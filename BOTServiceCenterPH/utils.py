import re
from unidecode import unidecode

def normalize_text(text: str) -> str:
    """Quita acentos, pasa a minúsculas, elimina puntuación y espacios extra."""
    text = unidecode(text or "")
    text = text.lower()
    text = re.sub(r'[^a-z0-9\s]', '', text)
    return re.sub(r'\s+', ' ', text).strip()
