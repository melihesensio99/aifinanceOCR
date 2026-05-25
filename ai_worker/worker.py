import pika
import json
import time
import requests
import cv2
import pytesseract
from PIL import Image

# Tesseract yolunu (Windows için) ayarlayın.
pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'

RABBITMQ_HOST = 'localhost'
QUEUE_NAME = 'receipt_queue_v2'
API_URL = 'https://localhost:7133/api/transaction/ai-webhook'
MISTRAL_API_KEY = 'ETf07ci39I6KV5o2sIsXNUl9a3xoTqCj'

def parse_receipt_text(text):
    print("Orijinal Metin (Mistral'e Gönderiliyor):")
    print("----------------")
    print(text)
    print("----------------")

    prompt = f"""
    Sen bir finans asistanısın. Aşağıdaki OCR metni bir market veya restoran fişine ait. Metin hatalı (Örn: '0' yerine 'O', virgül yerine nokta, '*9,90' gibi karakterler) olabilir.
    GÖREVİN: Metni analiz et, GERÇEK ürün isimlerini ve GERÇEK fiyatlarını bul. 
    DİKKAT: ASLA uydurma fiyat (Örn: 90.0, 50.0) yazma! Fiyatlar metnin içinde '*9,90', '15,90', 'x10 50' gibi formatlarda gizlidir. Onları bul ve ondalıklı sayıya (Örn: 9.90, 15.90) çevir.

    Şu formata tam olarak uyan BİR JSON döndür (başka hiçbir metin veya markdown ekleme):
    {{
        "title": "Fiş Başlığı veya Market Adı",
        "date": "YYYY-MM-DDT00:00:00Z",
        "amount": 82.55,
        "items": [
            {{"name": "BİRŞAH 500G MEY.YOĞ.", "price": 9.90}},
            {{"name": "METİNDEKİ DİĞER ÜRÜN", "price": 0.00}}
        ],
        "categoryId": "11111111-1111-1111-1111-111111111111"
    }}
    Notlar:
    - categoryId: Taksi, ulaşım ise "22222222-2222-2222-2222-222222222222", diğer her şey için "11111111-1111-1111-1111-111111111111" yap.
    - Tarih bulamazsan "2026-05-22T00:00:00Z" kullan.
    - Fiyatları sadece ondalıklı sayı (float) olarak ver.

    Fiş Metni:
    {text}
    """

    headers = {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': f'Bearer {MISTRAL_API_KEY}'
    }

    data = {
        'model': 'open-mistral-nemo',
        'response_format': {'type': 'json_object'},
        'messages': [
            {'role': 'user', 'content': prompt}
        ]
    }

    try:
        response = requests.post('https://api.mistral.ai/v1/chat/completions', headers=headers, json=data)
        response_json = response.json()
        
        if 'choices' not in response_json:
            print(f"Mistral Beklenmeyen Yanıt: {response_json}")
            return None
            
        content = response_json['choices'][0]['message']['content']
        
        # Markdown kod bloklarını (```json ... ```) temizle
        content = content.replace('```json', '').replace('```', '').strip()
        parsed = json.loads(content)
        
        # Öğeleri açıklamaya dönüştür
        description = "Mistral AI Analizi\n\nAlınan Ürünler:\n"
        items = parsed.get("items", [])
        for item in items:
            name = item.get("name", "Bilinmeyen Ürün")
            price = item.get("price", 0.0)
            description += f"• {name} - {price} TL\n"
        
        return {
            "title": parsed.get("title", "Bilinmeyen Fiş"),
            "amount": parsed.get("amount", 0.0),
            "type": "Expense",
            "date": parsed.get("date", "2026-05-22T00:00:00Z"),
            "description": description.strip(),
            "categoryId": parsed.get("categoryId", "11111111-1111-1111-1111-111111111111")
        }
    except Exception as e:
        print(f"Mistral AI Hatası: {e}")
        return None

def process_image(image_path):
    try:
        # Resmi OpenCV ile oku ve gri tonlamaya çevir
        img = cv2.imread(image_path)
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        
        # Yeniden Boyutlandırma (Scaling) yazıları netleştirir
        gray = cv2.resize(gray, None, fx=2, fy=2, interpolation=cv2.INTER_CUBIC)
        
        # DİKKAT: Tesseract LSTM motoru kendi içinde Adaptive Threshold kullanır. 
        # Sizin yazdığınız Otsu Threshold filtresi fişin ışığına göre resmi tamamen
        # bembeyaz (veya simsiyah) yapmış, bu yüzden metin BOMBOŞ dönmüş.
        # Bu yüzden filtreyi kaldırıp sadece büyütülmüş resmi okutuyoruz:
        # psm 4: Assume a single column of text of variable sizes (Fişler için idealdir)
        # psm 6: Assume a single uniform block of text
        custom_config = r'--oem 3 --psm 6'
        text = pytesseract.image_to_string(gray, lang='tur', config=custom_config)
        return parse_receipt_text(text)
    except Exception as e:
        print(f"OCR Hatası: {e}")
        return None

def callback(ch, method, properties, body):
    print(" [x] Yeni mesaj alındı!")
    event = json.loads(body)
    
    # C# System.Text.Json varsayılan olarak camelCase gönderir.
    image_path = event.get('ImagePath') or event.get('imagePath')
    user_id = event.get('UserId') or event.get('userId')

    if not image_path:
        print("Resim yolu bulunamadı.")
        ch.basic_ack(delivery_tag=method.delivery_tag)
        return

    print(f"Resim işleniyor: {image_path}")
    parsed_data = process_image(image_path)

    if parsed_data:
        parsed_data["UserId"] = user_id
        print(f"Elde edilen veriler: {parsed_data}")
        try:
            headers = {
                'Content-Type': 'application/json',
                'x-ai-api-key': 'secret_ai_key_123'
            }
            response = requests.post(API_URL, json=parsed_data, headers=headers, verify=False)
            if response.status_code == 200 or response.status_code == 202:
                print("Harcama C# API'ye başarıyla kaydedildi!")
                res_data = response.json()
                transaction_id = res_data.get('transaction', {}).get('id')
                
                # Eğer kategori Market/Gıda ise (1111...111), Scraping Kuyruğuna at!
                if transaction_id and parsed_data.get("categoryId") == "11111111-1111-1111-1111-111111111111":
                    items_only = [line.split('-')[0].replace('•', '').strip() for line in parsed_data["description"].split('\n') if line.startswith('•')]
                    
                    scraping_msg = {
                        "transactionId": transaction_id,
                        "items": items_only
                    }
                    
                    # Aynı connection üzerinden ikinci kuyruğa fırlat
                    ch.queue_declare(queue='price_scraping_queue', durable=True)
                    ch.basic_publish(
                        exchange='',
                        routing_key='price_scraping_queue',
                        body=json.dumps(scraping_msg),
                        properties=pika.BasicProperties(
                            delivery_mode=2,  # kalıcı mesaj
                        )
                    )
                    print(f" Scraping kuyruğuna {len(items_only)} ürün gönderildi!")
                else:
                    print(" Market/Gıda dışı kategori, fiyat taraması atlandı.")
            else:
                print(f"C# API Hatası: {response.status_code} - {response.text}")
        except Exception as e:
            print(f"API İstek Hatası: {e}")

    ch.basic_ack(delivery_tag=method.delivery_tag)

def main():
    try:
        connection = pika.BlockingConnection(pika.ConnectionParameters(host=RABBITMQ_HOST))
        channel = connection.channel()
        channel.queue_declare(queue=QUEUE_NAME, durable=True)
        
        channel.basic_qos(prefetch_count=1)
        channel.basic_consume(queue=QUEUE_NAME, on_message_callback=callback)

        print(' [*] Fiş mesajları bekleniyor. Çıkmak için CTRL+C basın.')
        channel.start_consuming()
    except Exception as e:
        print(f"RabbitMQ Bağlantı Hatası: {e}")
        print("Yeniden deneniyor...")
        time.sleep(5)
        main()

if __name__ == '__main__':
    main()
