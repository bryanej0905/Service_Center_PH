import os
from dotenv import load_dotenv
import openai

# Carga variables de entorno desde .env (si existe) y entorno del sistema
load_dotenv()

# Obtén la clave de OpenAI desde la variable de entorno
openai.api_key = os.getenv("OPENAI_API_KEY")

# Función de fallback usando RAG + ChatCompletion
def generate_rag_response(question: str, context_pairs: list[tuple[str, str]]) -> str:
    """
    Genera una respuesta usando OpenAI Chat completions, basándose en pares (pregunta, respuesta) como contexto.

    question: la consulta del usuario sin normalizar.
    context_pairs: lista de tuplas (pregunta_norm, respuesta) obtenidas del embedding matcher.

    Devuelve el texto de la respuesta generada.
    """
    # Construir contexto legible
    context_text = "\n".join([f"Q: {q}\nA: {a}" for q, a in context_pairs])

    messages = [
        {"role": "system", "content": "Eres un asistente de soporte interno de Pizza Hut. Usa solo la información proporcionada a continuación para responder preguntas de los empleados."},
        {"role": "user", "content": (
            f"Información disponible:\n{context_text}\n\n"
            f"Pregunta: {question}\n"
            "Si no sabes la respuesta exacta, di que no tienes la información."
        )}
    ]

    resp = openai.ChatCompletion.create(
        model="gpt-3.5-turbo",
        messages=messages,
        max_tokens=200,
        temperature=0
    )
    return resp.choices[0].message.content.strip()
