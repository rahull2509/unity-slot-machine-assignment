#!/usr/bin/env python3
"""
Simple HTTP server configured with gzip Content-Encoding headers for Unity WebGL builds.
Run:
    python serve_webgl.py [port]
"""

import sys
import os
import http.server
import socketserver

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8080
DIRECTORY = os.path.join(os.path.dirname(__file__), "Build", "WebGL")

class UnityWebGLHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=DIRECTORY, **kwargs)

    def end_headers(self):
        # Enable Cross-Origin Isolation for high-performance wasm features
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        self.send_header("Access-Control-Allow-Origin", "*")
        super().end_headers()

    def guess_type(self, path):
        ctype = super().guess_type(path)
        if path.endswith(".wasm") or path.endswith(".wasm.gz"):
            return "application/wasm"
        elif path.endswith(".data") or path.endswith(".data.gz"):
            return "application/octet-stream"
        elif path.endswith(".js") or path.endswith(".js.gz"):
            return "application/javascript"
        return ctype

    def send_head(self):
        path = self.translate_path(self.path)
        if os.path.isfile(path) and path.endswith(".gz"):
            self.send_response(200)
            ctype = self.guess_type(path[:-3])
            self.send_header("Content-Type", ctype)
            self.send_header("Content-Encoding", "gzip")
            fs = os.fstat(os.open(path, os.O_RDONLY))
            self.send_header("Content-Length", str(fs.st_size))
            self.end_headers()
            return open(path, "rb")
        return super().send_head()

if __name__ == "__main__":
    if not os.path.exists(DIRECTORY):
        print(f"Error: Directory '{DIRECTORY}' does not exist.")
        sys.exit(1)

    print(f"Serving Unity WebGL build from: {DIRECTORY}")
    print(f"Open your browser at: http://localhost:{PORT}")
    print("Press Ctrl+C to stop the server.")

    socketserver.TCPServer.allow_reuse_address = True
    with socketserver.TCPServer(("", PORT), UnityWebGLHandler) as httpd:
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nServer stopped.")

