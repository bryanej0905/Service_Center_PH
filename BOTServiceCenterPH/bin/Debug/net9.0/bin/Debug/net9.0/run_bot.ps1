# ─── CONFIGURACIÓN ───
$pythonVersion = "3.11.6"
$pythonFolderName = "PythonInstaller311"
$pythonUrl = "https://www.python.org/ftp/python/$pythonVersion/python-$pythonVersion-embed-amd64.zip"
$zipPath = "$env:TEMP\python-embed.zip"
$pythonDir = "$env:LOCALAPPDATA\Programs\Python\$pythonFolderName"
$pythonExe = "$pythonDir\python.exe"
$pydist = "$pythonDir\python311._pth"
$appScript = "$PSScriptRoot\app.py"
$requirements = "$PSScriptRoot\requirements.txt"

# ─── FUNCIONES ───
function Install-Python {
    Write-Host "[INFO] Instalando Python embebido..."
    Invoke-WebRequest -Uri $pythonUrl -OutFile $zipPath
    Expand-Archive -Path $zipPath -DestinationPath $pythonDir -Force
    Remove-Item $zipPath

    # Activar import site en el archivo ._pth
    (Get-Content "$pydist") -replace '#import site', 'import site' | Set-Content "$pydist"

    Write-Host "[INFO] Python instalado en $pythonDir"
}

function Ensure-Pip {
    if (-Not (Test-Path "$pythonDir\Scripts\pip.exe")) {
        Write-Host "[INFO] Instalando pip..."
        Invoke-WebRequest "https://bootstrap.pypa.io/get-pip.py" -OutFile "$pythonDir\get-pip.py"
        & $pythonExe "$pythonDir\get-pip.py"
        Remove-Item "$pythonDir\get-pip.py"
    } else {
        Write-Host "[INFO] pip ya está instalado."
    }

    # Agregar Scripts al PATH de esta sesión
    $env:PATH += ";$pythonDir\Scripts"
}

function Install-Requirements {
    Write-Host "[INFO] Instalando dependencias desde requirements.txt..."
    & $pythonExe -m pip install --upgrade pip
    & $pythonExe -m pip install -r $requirements

    Write-Host "[INFO] Verificando e instalando manualmente 'python-dotenv'..."
    & $pythonExe -m pip install python-dotenv
}

function Ensure-SpacyModel {
    Write-Host "[INFO] Verificando modelo de spaCy 'es_core_news_sm'..."
    & $pythonExe -c "import spacy; spacy.load('es_core_news_sm')" 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[INFO] Modelo 'es_core_news_sm' no encontrado. Descargando..."
        & $pythonExe -m spacy download es_core_news_sm
    } else {
        Write-Host "[INFO] Modelo 'es_core_news_sm' ya está instalado."
    }
}

# ─── PROCESO ───
if (-Not (Test-Path $pythonExe)) {
    Install-Python
} else {
    Write-Host "[INFO] Python ya está instalado en $pythonDir"
}

Ensure-Pip
Install-Requirements
Ensure-SpacyModel

# ─── Ejecutar aplicación ───
Write-Host "`n🚀 Ejecutando el bot..."
& $pythonExe $appScript

# ─── Mantener consola abierta ───
Write-Host "`nPresioná ENTER para cerrar..."
Read-Host
