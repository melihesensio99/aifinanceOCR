# 2210656062 KAĞAN KAHRAMAN
# Python Iterator ve Generator yapıları ile External Merge Sort Uygulaması

import os
import tempfile
import heapq
import time
import tracemalloc

# ---------------------------------------------------------
# BÖLÜM 1: EAGER VS LAZY EVALUATION (FARKIN ÖRNEKLENMESİ)
# ---------------------------------------------------------
def eager_evaluation_example(n):
    """Eager (Hevesli) Evaluation: Tüm veriyi anında belleğe yükler."""
    print("Eager Evaluation başlıyor...")
    # List comprehension tüm listeyi bellekte aynı anda oluşturur.
    return [x ** 2 for x in range(n)]

def lazy_evaluation_example(n):
    """Lazy (Tembel) Evaluation: Veriyi sadece ihtiyaç duyuldukça (generator ile) üretir."""
    print("Lazy Evaluation başlıyor...")
    # Generator expression veriyi tek tek üretir, belleği doldurmaz.
    return (x ** 2 for x in range(n))

def demonstrate_evaluation_differences():
    print("--- LAZY VS EAGER EVALUATION ---")
    N = 5000000 # 5 Milyon kayıt
    
    # Eager Test
    tracemalloc.start()
    eager_data = eager_evaluation_example(N)
    current, peak = tracemalloc.get_traced_memory()
    print(f"Eager Evaluation Bellek Kullanımı: {peak / 10**6:.2f} MB")
    tracemalloc.stop()
    del eager_data # Belleği temizle

    # Lazy Test
    tracemalloc.start()
    lazy_data = lazy_evaluation_example(N)
    current, peak = tracemalloc.get_traced_memory()
    print(f"Lazy Evaluation Bellek Kullanımı: {peak / 10**6:.2f} MB")
    tracemalloc.stop()
    print("--------------------------------\n")

# ---------------------------------------------------------
# BÖLÜM 2: EXTERNAL MERGE SORT
# ---------------------------------------------------------

def create_large_dummy_file(filename, num_lines):
    """Test için büyük bir dosya oluşturur."""
    import random
    with open(filename, 'w') as f:
        for _ in range(num_lines):
            f.write(f"{random.randint(1, 1000000)}\n")

def split_and_sort_chunks(input_file, chunk_size=10000):
    """Büyük dosyayı okuyup, chunk_size kadar parçalara böler, sıralar ve geçici dosyalara yazar."""
    temp_files = []
    with open(input_file, 'r') as f:
        chunk = []
        for line in f:
            chunk.append(int(line.strip()))
            if len(chunk) >= chunk_size:
                chunk.sort()
                temp_files.append(write_chunk_to_temp(chunk))
                chunk = [] # Belleği boşalt
        
        # Kalan son parçayı işle
        if chunk:
            chunk.sort()
            temp_files.append(write_chunk_to_temp(chunk))
            
    return temp_files

def write_chunk_to_temp(chunk):
    """Sıralı listeyi geçici bir dosyaya yazar ve dosya adını döner."""
    temp_file = tempfile.NamedTemporaryFile(delete=False, mode='w', dir='.')
    for item in chunk:
        temp_file.write(f"{item}\n")
    temp_file.close()
    return temp_file.name

def read_numbers_from_file(filename):
    """Bir dosyadan satır satır sayı okuyan GENERATOR (Lazy Evaluation)."""
    with open(filename, 'r') as f:
        for line in f:
            yield int(line.strip())

def external_merge_sort(input_file, output_file, chunk_size=10000):
    """Ana external merge sort algoritması."""
    print("1. Dosya parçalara bölünüyor ve sıralanıyor...")
    temp_files = split_and_sort_chunks(input_file, chunk_size)
    print(f"   Toplam {len(temp_files)} adet geçici (temp) dosya oluşturuldu.")
    
    print("2. Generator'lar ile Merge işlemi başlatılıyor...")
    # Her bir temp dosya için bir generator (iterator) oluşturuyoruz.
    # Dosyalar belleğe tamamen yüklenmez, satır satır okunur.
    generators = [read_numbers_from_file(temp_file) for temp_file in temp_files]
    
    # heapq.merge, birden fazla sıralı iterator'ı (generator) birleştirip sıralı tek bir iterator döner.
    merged_generator = heapq.merge(*generators)
    
    print("3. Sıralanmış veri çıkış dosyasına yazılıyor...")
    with open(output_file, 'w') as out_f:
        for number in merged_generator:
            out_f.write(f"{number}\n")
            
    # Temizlik: Geçici dosyaları sil
    for temp_file in temp_files:
        os.remove(temp_file)
    print("İşlem tamamlandı. Geçici dosyalar silindi.")

# ---------------------------------------------------------
# ANA PROGRAM ÇALIŞTIRMA
# ---------------------------------------------------------
if __name__ == "__main__":
    print("Öğrenci: 2210656062 KAĞAN KAHRAMAN\n")
    
    # 1. Kavramların gösterimi
    demonstrate_evaluation_differences()
    
    # 2. External Merge Sort Testi
    input_txt = "large_input.txt"
    output_txt = "sorted_output.txt"
    
    print("Test için büyük dosya oluşturuluyor (100.000 satır)...")
    create_large_dummy_file(input_txt, 100000)
    
    print("\n--- EXTERNAL MERGE SORT BAŞLIYOR ---")
    tracemalloc.start()
    
    external_merge_sort(input_txt, output_txt, chunk_size=20000)
    
    current, peak = tracemalloc.get_traced_memory()
    print(f"\nExternal Merge Sort Sırasında Maksimum Bellek Kullanımı (Peak): {peak / 10**6:.2f} MB")
    print("Gördüğünüz gibi, 100.000 satırlık dosya işlenirken bile bellek sabit ve çok düşük kaldı (Generator avantajı).")
    tracemalloc.stop()
    
    # Test bittikten sonra input dosyasını silebiliriz
    if os.path.exists(input_txt):
        os.remove(input_txt)
