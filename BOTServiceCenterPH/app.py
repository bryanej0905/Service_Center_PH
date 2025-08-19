import subprocess
import sys
import os
import threading
import signal
import socket
sys.path.append(os.path.dirname(os.path.abspath(__file__)))
# ─── Instalar requirements ───
def install_requirements():
    requirements_file = os.path.join(os.path.dirname(__file__), 'requirements.txt')
    print("[INFO] Verificando e instalando dependencias desde requirements.txt...")
    try:
        subprocess.check_call([sys.executable, "-m", "pip", "install", "-r", requirements_file])
        print("[INFO] Dependencias instaladas correctamente.")
    except subprocess.CalledProcessError:
        print("[ERROR] Falló la instalación de dependencias.")
        sys.exit(1)

install_requirements()

# ─── Imports ───
from dotenv import load_dotenv
from flask import Flask, request
from faq_loader import FAQLoader
from embedding_matcher import EmbeddingMatcher
from colorama import init, Fore
from PIL import Image
import pystray
import routes

# ─── IP Local ───
def get_local_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(("8.8.8.8", 80))
        return s.getsockname()[0]
    except Exception:
        return "127.0.0.1"
    finally:
        s.close()

# ─── Icono en bandeja ───
def create_tray_icon():
    try:
        image = Image.open("favicon.ico")
    except FileNotFoundError:
        print(Fore.RED + "[ERROR] Icono pizza.png no encontrado.")
        return

    def on_quit(icon, item):
        print(Fore.RED + "⏹ Cerrando servidor desde bandeja...")
        icon.stop()
        os.kill(os.getpid(), signal.SIGINT)

    menu = pystray.Menu(pystray.MenuItem("Salir", on_quit))
    icon = pystray.Icon("flask_bot", image, "FLASK BOT", menu)
    threading.Thread(target=icon.run, daemon=True).start()

# ─── Flask App ───
def create_app():
    load_dotenv()
    init(autoreset=True)

    app = Flask(__name__)
    app.secret_key = 'supersecretkey123'

    base_dir = os.path.dirname(os.path.abspath(__file__))
    data_dir = os.path.join(base_dir, 'data')

    loader = FAQLoader(data_dir)
    matcher_inst = EmbeddingMatcher(loader.questions, loader.get_answer)
    routes.faq_loader = loader
    routes.matcher = matcher_inst

    app.register_blueprint(routes.tchat_bp)

    # Limpiar consola
    os.system('cls' if os.name == 'nt' else 'clear')
    terminal_width = 80
    ip = get_local_ip()

    # Banner bonito
    print(Fore.CYAN + "+" + "-" * (terminal_width - 2) + "+")
    print(Fore.CYAN + "|{:^77}|".format("🍕 BOT DEL SERVICECENTERPH INICIADO"))
    print(Fore.CYAN + "+" + "-" * (terminal_width - 2) + "+")
    print(Fore.YELLOW + "|{:^77}|".format("📌 RUTAS REGISTRADAS"))
    print(Fore.CYAN + "+" + "-" * (terminal_width - 2) + "+")

    for rule in app.url_map.iter_rules():
        endpoint = f"{rule.endpoint}"
        path = f"{rule.rule}"
        line = f"{endpoint:30} → {path}"
        print(Fore.GREEN + "| " + line.ljust(terminal_width - 4) + " |")

    print(Fore.CYAN + "+" + "-" * (terminal_width - 2) + "+")
    print(Fore.MAGENTA + f"| 🌐 Escuchando en: http://{ip}:5000".ljust(terminal_width - 3) + " |")
    print(Fore.CYAN + "+" + "-" * (terminal_width - 2) + "+")

    return app

# ─── Cierre ───
def shutdown_server():
    func = request.environ.get('werkzeug.server.shutdown')
    if func:
        func()

def wait_for_exit():
    input(Fore.RED + "\n🔴 Presioná ENTER para cerrar el servidor...\n")
    print(Fore.RED + "⏹ Cerrando servidor...")
    os.kill(os.getpid(), signal.SIGINT)

# ─── Ejecutar ───
app = create_app()

if __name__ == '__main__':
    threading.Thread(target=wait_for_exit, daemon=True).start()
    create_tray_icon()
    app.run(debug=True, host=get_local_ip(), port=5000)

