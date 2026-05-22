import pika
import json
import time
import requests
import re
import cv2
import pytesseract
from PIL import Image

# Tesseract yolunu (Windows için) ayarlayın.
pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'

RABBITMQ_HOST = 'localhost'
QUEUE_NAME = 'receipt_queue_v2'
API_URL = 'http://localhost:5294/api/transaction/ai-webhook'

def parse_receipt_text(text):
    print("Orijinal Metin:")
    print("----------------")
    print(text)
    print("----------------")

    # Basit Regex ve Keyword Eşleştirme
    # 1. Tarih Bulma (GG.AA.YYYY veya GG/AA/YYYY)
    date_match = re.search(r'\b(\d{2})[./-](\d{2})[./-](\d{4})\b', text)
    date_str = f"{date_match.group(3)}-{date_match.group(2)}-{date_match.group(1)}T00:00:00Z" if date_match else "2026-05-22T00:00:00Z"

    # 2. Tutar Bulma (TOPLAM, TUTAR gibi kelimelerden sonra gelen sayı)
    amount_match = re.search(r'(TOPLAM|TUTAR|KDV DAH[Iİ]L)\s*[:=]?\s*[\*]*\s*(\d+[,.]\d{2})', text, re.IGNORECASE)
    amount = 0.0
    if amount_match:
        amount_str = amount_match.group(2).replace(',', '.')
        amount = float(amount_str)

    # 3. Başlık (Mekan) Bulma (İlk satırı al)
    lines = [line.strip() for line in text.split('\n') if line.strip()]
    title = lines[0] if lines else "Bilinmeyen Fiş"

    # 4. Kategori Bulma (Basit keyword eşleştirme)
    # 22222222-2222-2222-2222-222222222222 = Ulaşım, 11111111-1111-1111-1111-111111111111 = Yemek
    text_upper = text.upper()
    category_id = "11111111-1111-1111-1111-111111111111" # Default Food
    if any(keyword in text_upper for keyword in ["TAKSİ", "PETROL", "BİLET", "OTOBÜS", "MARMARAY"]):
        category_id = "22222222-2222-2222-2222-222222222222" # Transport

    return {
        "title": title,
        "amount": amount,
        "type": "Expense",
        "date": date_str,
        "description": "Otomatik OCR Analizi",
        "categoryId": category_id
    }

def process_image(image_path):
    try:
        # Resmi OpenCV ile oku ve gri tonlamaya çevir (OCR başarısını artırır)
        img = cv2.imread(image_path)
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        
        # Tesseract ile oku (Türkçe dil desteği: lang='tur')
        text = pytesseract.image_to_string(gray, lang='tur')
        return parse_receipt_text(text)
    except Exception as e:
        print(f"OCR Hatası: {e}")
        return None

def callback(ch, method, properties, body):
    print(" [x] Yeni mesaj alındı!")
    event = json.loads(body)
    image_path = event.get('ImagePath')
    user_id = event.get('UserId')

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
