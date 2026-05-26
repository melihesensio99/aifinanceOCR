using System;

namespace AIFinancePlatform.Domain.Entities;

public class ProductPriceCache
{
    public Guid Id { get; set; }
    
    // Aranan kelime (Örn: "BİRŞAH 500G MEY.YOĞ.")
    public string SearchTerm { get; set; }
    
    // Bulunan fiyat metni (Örn: "49,95 TL")
    public string Price { get; set; }
    
    // Verinin önbelleğe alınma zamanı
    public DateTime CreatedAt { get; set; }
}
