import re

def update_index_html():
    # Читаємо файл
    with open('index.html', 'r', encoding='utf-8') as file:
        content = file.read()
    
    # Визначаємо CSS стилі, які потрібно додати
    styles_to_add = """    <style>
      body { 
        margin: 0; 
        padding: 0; 
        overflow: hidden;
      }
      #unity-container { 
        width: 100vw; 
        height: 100vh; 
        position: absolute;
        overflow: hidden;
      }
      #unity-canvas { 
        width: 100% !important; 
        height: 100% !important;
      }
    </style>"""

    # Додаємо стилі перед закриваючим тегом head
    if '<style>' not in content:
        content = content.replace('</head>', f'{styles_to_add}\n  </head>')
    
    # Змінюємо canvas tag
    content = re.sub(
        r'<canvas id="unity-canvas"[^>]*>',
        '<canvas id="unity-canvas" tabindex="-1">',
        content
    )
    
    # Видаляємо/коментуємо фіксовані розміри canvas
    content = content.replace(
        'canvas.style.width = "1880px";\n        canvas.style.height = "930px";',
        '// Видаляємо фіксовані розміри canvas\n        // canvas.style.width = "1880px";\n        // canvas.style.height = "930px";'
    )
    
    # Зберігаємо зміни
    with open('index.html', 'w', encoding='utf-8') as file:
        file.write(content)

if __name__ == "__main__":
    try:
        update_index_html()
        print("✅ Успішно оновлено index.html")
        print("   - Додано стилі для повноекранного відображення")
        print("   - Видалено фіксовані розміри canvas")
        print("   - Приховано скролбари")
    except Exception as e:
        print(f"❌ Помилка при оновленні файлу: {str(e)}") 