import os
import re
import pandas as pd

"""
Script para analizar las preguntas sin respuesta registradas en logs/unanswered.log.
Genera un DataFrame con conteo de preguntas y lo muestra por consola.
"""

def main():
    # Ruta al log de consultas sin respuesta
    base_dir = os.path.dirname(os.path.abspath(__file__))
    log_path = os.path.join(base_dir, 'logs', 'unanswered.log')

    # Verificar existencia
    if not os.path.exists(log_path):
        print(f"No se encontró el archivo de log en {log_path}")
        return

    # Leer todas las líneas
    with open(log_path, encoding='utf-8') as f:
        lines = f.readlines()

    # Extraer timestamp y pregunta
    pattern = re.compile(r"^(?P<timestamp>[^ ]+) No answer for: '(?P<question>.*)'$")
    records = []
    for line in lines:
        match = pattern.match(line.strip())
        if match:
            records.append(match.groupdict())

    # Construir DataFrame
    df = pd.DataFrame(records)
    if df.empty:
        print("No hay preguntas sin respuesta registradas.")
        return

    # Contar frecuencias y ordenar
    report = df['question'].value_counts().reset_index()
    report.columns = ['question', 'count']

    # Mostrar resultado
    print("Preguntas sin respuesta más frecuentes:")
    print(report.to_string(index=False))

if __name__ == '__main__':
    main()
