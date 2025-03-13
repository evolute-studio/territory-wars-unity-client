import http.server
import socketserver
import os
import ssl
import argparse

class UnityWebGLHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        # Встановлюємо поточну директорію як directory
        super().__init__(*args, directory=os.path.dirname(os.path.abspath(__file__)), **kwargs)

    def do_GET(self):
        """Handle GET requests with proper headers for .br files"""
        # Розширений дебаг-вивід
        print(f"\nReceived request for: {self.path}")
        print(f"Current working directory: {os.getcwd()}")
        print(f"Server directory: {self.directory}")
        print(f"Translated path: {self.translate_path(self.path)}")
        print(f"File exists: {os.path.exists(self.translate_path(self.path))}")
        
        # ... rest of the existing do_GET code ...

# Налаштування аргументів
parser = argparse.ArgumentParser(description='Запустити Unity WebGL сервер')
parser.add_argument('--port', type=int, default=8000, help='Порт для запуску сервера (за замовчуванням: 8000)')
args = parser.parse_args()

PORT = args.port
Handler = UnityWebGLHandler

print(f"\nStarting Unity WebGL server on port {PORT}")
print(f"Server directory: {os.path.dirname(os.path.abspath(__file__))}")
print(f"Server will handle .br files with proper Content-Encoding")
print(f"Open http://localhost:{PORT} in your browser")

# ... rest of the existing code ...