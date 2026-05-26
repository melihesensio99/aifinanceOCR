description = """
Mistral AI Analizi

Alınan Ürünler:
• BİRŞAH 500G MEY.YOĞ. - 9.9 TL
• KRUVASAN 55G7DAYS O - 3.5 TL
"""
items_only = [line.split('-')[0].replace('•', '').strip() for line in description.split('\n') if line.startswith('•')]
print(items_only)
