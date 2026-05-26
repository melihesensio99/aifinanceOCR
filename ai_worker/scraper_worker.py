import pika
import json
import time
import random
import requests
from bs4 import BeautifulSoup
import urllib.parse

RABBITMQ_HOST = 'localhost'
QUEUE_NAME = 'price_scraping_queue'
API_URL = 'https://localhost:7133/api/transaction/ai-webhook'
CACHE_API_URL = 'https://localhost:7133/api/pricecache'

# Simüle Edilmiş Ücretsiz Proxy Havuzu (Gerçek dünyada BrightData, Oxylabs gibi ücretli servisler kullanılır)
PROXIES = [
    # Format: {'http': 'http://ip:port', 'https': 'http://ip:port'}
    None, # Kendi IP'miz (Test için)
    # {'http': 'http://185.199.229.156:8080', 'https': 'http://185.199.229.156:8080'},
    # {'http': 'http://148.251.20.79:8080', 'https': 'http://148.251.20.79:8080'}
]

def get_carrefour_price(product_name):
    # 1. Önce kendi veritabanımızdaki Hafızaya (Cache) sor!
    try:
        headers = {'x-ai-api-key': 'secret_ai_key_123'}
        cache_res = requests.get(f"{CACHE_API_URL}?term={urllib.parse.quote(product_name)}", headers=headers, verify=False)
        if cache_res.status_code == 200:
            cached_price = cache_res.json().get('price')
            if cached_price:
                print("   [CACHE] Fiyat hafızadan getirildi! (Carrefour'a istek atılmadı)")
                return cached_price
    except Exception as e:
        print(f"   [CACHE HATA] {e}")

    # 2. Eğer hafızada yoksa, Proxy Havuzundan rastgele bir proxy seç
    proxy = random.choice(PROXIES)
    if proxy:
        print(f"   [PROXY] İstek şu proxy üzerinden çıkıyor: {proxy['http']}")
    
    try:
        url = f'https://www.carrefoursa.com/search/?text={urllib.parse.quote(product_name)}'
        req_headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'}
        
        # Proxy kullanarak isteği gönder
        res = requests.get(url, headers=req_headers, proxies=proxy, timeout=7)
        if res.status_code == 200:
            soup = BeautifulSoup(res.text, 'html.parser')
            items = soup.find_all('span', class_='item-price')
            if items:
                price = items[0].text.strip()
                
                # 3. Bulunan yeni fiyatı veritabanındaki Hafızaya (Cache) kaydet
                try:
                    payload = {"SearchTerm": product_name, "Price": price}
                    requests.post(CACHE_API_URL, json=payload, headers=headers, verify=False)
                except Exception as e:
                    pass
                    
                return price
    except Exception as e:
        print(f"Scraping Hatası ({product_name}): {e}")
    return None

def callback(ch, method, properties, body):
    print(" [x] Yeni Scraping görevi alındı!")
    event = json.loads(body)
    
    transaction_id = event.get('transactionId')
    items = event.get('items', [])
    
    if not transaction_id or not items:
        ch.basic_ack(delivery_tag=method.delivery_tag)
        return
        
    print(f" İşlem ID: {transaction_id} için {len(items)} ürün taranıyor...")
    
    appended_text = ""
    for item in items:
        # Arama kelimesini biraz temizleyelim (İlk 2 kelimesi yeterli olabilir)
        search_term = " ".join(item.split()[:3])
        print(f" Aranıyor: {search_term} (CarrefourSA)")
        
        price = get_carrefour_price(search_term)
        if price:
            appended_text += f"\n💡 Fiyat Alarmı: {item} -> CarrefourSA'da {price}"
            print(f"   Bulundu: {price}")
        else:
            print(f"   Bulunamadı.")
            
        time.sleep(1) # Banlanmamak için 1 saniye bekle
        
    if appended_text:
        try:
            update_url = f"{API_URL}/{transaction_id}/append-description"
            payload = {"TextToAppend": "\n--- CANLI PİYASA TARAMASI ---" + appended_text}
            headers = {
                'Content-Type': 'application/json',
                'x-ai-api-key': 'secret_ai_key_123'
            }
            res = requests.put(update_url, json=payload, headers=headers, verify=False)
            if res.status_code == 200:
                print(" C# API'ye fiyat alarmları başarıyla eklendi!")
            else:
                print(f" C# API Hatası: {res.status_code} - {res.text}")
        except Exception as e:
            print(f" API İstek Hatası: {e}")
            
    ch.basic_ack(delivery_tag=method.delivery_tag)

def main():
    try:
        connection = pika.BlockingConnection(pika.ConnectionParameters(host=RABBITMQ_HOST))
        channel = connection.channel()
        channel.queue_declare(queue=QUEUE_NAME, durable=True)
        
        channel.basic_qos(prefetch_count=1)
        channel.basic_consume(queue=QUEUE_NAME, on_message_callback=callback)

        print(' [*] Scraping Worker çalışıyor. Çıkmak için CTRL+C basın.')
        channel.start_consuming()
    except Exception as e:
        print(f"RabbitMQ Bağlantı Hatası: {e}")
        print("Yeniden deneniyor...")
        time.sleep(5)
        main()

if __name__ == '__main__':
    main()
