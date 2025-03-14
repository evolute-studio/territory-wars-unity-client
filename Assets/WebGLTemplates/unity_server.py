import http.server
import socketserver
import os
import ssl

class UnityWebGLHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        """Handle GET requests with proper headers for .br files"""
        # Print request details for debugging
        print(f"\nReceived request for: {self.path}")
        
        # Handle .br files specially
        if self.path.endswith('.br'):
            try:
                # Get the file path
                path = self.translate_path(self.path)
                
                # Open and read the file
                f = open(path, 'rb')
                fs = os.fstat(f.fileno())
                
                # Send headers
                self.send_response(200)
                self.send_header('Content-Encoding', 'br')
                
                # Set correct content type based on original file
                if self.path.endswith('.js.br'):
                    self.send_header('Content-Type', 'application/javascript')
                elif self.path.endswith('.wasm.br'):
                    self.send_header('Content-Type', 'application/wasm')
                elif self.path.endswith('.data.br'):
                    self.send_header('Content-Type', 'application/octet-stream')
                
                self.send_header('Content-Length', str(fs.st_size))
                self.send_header('Cross-Origin-Embedder-Policy', 'require-corp')
                self.send_header('Cross-Origin-Opener-Policy', 'same-origin')
                self.send_header('Cross-Origin-Resource-Policy', 'cross-origin')
                self.send_header('Access-Control-Allow-Origin', '*')
                self.end_headers()
                
                # Print headers for debugging
                print("Sending headers for .br file:")
                print(f"Content-Encoding: br")
                print(f"Content-Length: {fs.st_size}")
                
                # Send the file content
                self.copyfile(f, self.wfile)
                f.close()
                return
            except Exception as e:
                print(f"Error handling .br file: {e}")
                self.send_error(500, f"Error handling .br file: {e}")
                return
        
        # For non-.br files, use default handling
        return http.server.SimpleHTTPRequestHandler.do_GET(self)
    
    def end_headers(self):
        """Add CORS and other required headers"""
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Cross-Origin-Embedder-Policy', 'require-corp')
        self.send_header('Cross-Origin-Opener-Policy', 'same-origin')
        self.send_header('Cross-Origin-Resource-Policy', 'cross-origin')
        super().end_headers()
    
    def guess_type(self, path):
        """Determine correct MIME type"""
        if path.endswith('.wasm'):
            return 'application/wasm'
        if path.endswith('.js'):
            return 'application/javascript'
        if path.endswith('.data'):
            return 'application/octet-stream'
        return super().guess_type(path)[0]

# Set up the server
PORT = 8000  # Change back to 8000
Handler = UnityWebGLHandler

print(f"\nStarting Unity WebGL server on port {PORT}")
print(f"Server will handle .br files with proper Content-Encoding")
print(f"Open http://localhost:{PORT} in your browser")

# Remove SSL context setup and just use basic server
with socketserver.TCPServer(("", PORT), Handler) as httpd:
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nShutting down server...")
        httpd.shutdown()
