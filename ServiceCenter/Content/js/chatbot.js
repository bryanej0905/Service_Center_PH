$(function () {
    const $chatPanel = $('#chatPanel');
    const $toggle = $('#chatToggleBtn');
    const $container = $('#chatContainer');
    const $input = $('#chatInput');
    const $sendBtn = $('#sendBtn');
    let $ticketBtn; 
    let formDiv;
    let botBaseUrl = '';

    $chatPanel.hide();
    fetchBotUrl();

    // Alternar el panel con el botón flotante
    $toggle.on('click', function () {
        $chatPanel.toggle();
    });

    function formatTime(date) {
        return date.getHours().toString().padStart(2, '0') + ':' +
            date.getMinutes().toString().padStart(2, '0');
    }

    function addMessage(text, isUser, suggestions = [], ticketOption = false, tituloSugerido = "") {
        const $msg = $('<div>').addClass('chat-message ' + (isUser ? 'user' : 'bot'));
        const avatarSrc = isUser ? '/Content/lib/img/pizza-logo.png' : '/Content/lib/img/bot.png';
        $msg.append($('<img>', {
            src: avatarSrc,
            alt: isUser ? 'Tú' : 'Bot',
            class: 'chat-avatar'
        }));

        const $bubbleCont = $('<div>').addClass('chat-bubble-container');
        $bubbleCont.append(
            $('<div>').addClass('chat-bubble ' + (isUser ? 'user' : 'bot')).text(text)
        );
        $bubbleCont.append(
            $('<span>').addClass('chat-timestamp').text(formatTime(new Date()))
        );
        $msg.append($bubbleCont);
        $container.append($msg);

        // 📌 Botón de sugerencias
        if (!isUser && suggestions.length > 0) {
            const $sugCont = $('<div>').addClass('suggestions');
            suggestions.forEach(s => {
                const $btn = $('<button>')
                    .addClass('btn btn-outline-secondary btn-sm m-1')
                    .text(s)
                    .click(() => sendMessage(s));
                $sugCont.append($btn);
            });
            $container.append($sugCont);
        }

        // 🎫 Botón para crear ticket si no hubo respuesta útil
        if (!isUser && text == "Lo siento, no tengo la información suficiente para eso.") {
            $ticketBtn = $('<button>')
                .addClass('btn btn-warning m-2')
                .attr('id', 'btnCrearTicketChatbot') // ID para eliminar luego
                .text('¿Deseas crear un ticket?')
                .click(() => showTicketForm("Ticket de ChatBot"));

            $container.append($ticketBtn);
        }

        $container.scrollTop($container.prop('scrollHeight'));
    }


    function showTyping() {
        const $typing = $(`
        <div id="typingIndicator" class="chat-message bot">
            <img src="/Content/lib/img/bot.png" alt="Bot" class="chat-avatar" />
            <div class="typing-indicator">
                <div class="typing-dots">
                    <div></div><div></div><div></div>
                </div>
            </div>
        </div>
    `);

      
        const $container = $('#chatContainer'); 
        if ($container.length) {
            $container.append($typing);
            $container.scrollTop($container.prop('scrollHeight'));
        } else {
            console.warn("Contenedor no encontrado");
        }
    }


    function hideTyping() {
        $('#typingIndicator').remove();
    }

    async function fetchBotUrl() {
        try {
            const res = await fetch('/ManageIPChatBots/GetUrl');
            const data = await res.json();
            if (data && data.url) {
                botBaseUrl = data.url;
                console.log('URL del bot:', botBaseUrl);
            } else {
                console.error('URL no válida desde el backend.');
            }
        } catch (err) {
            console.error('Error al obtener la URL del bot:', err);
        }
    }

    function normalizeQuestion(msg) {
        msg = msg.trim();

        // Agrega "¿" al inicio si no está
        if (!msg.startsWith('¿')) {
            msg = '¿' + msg;
        }

        // Agrega "?" al final si no está
        if (!msg.endsWith('?')) {
            msg += '?';
        }

        return msg;
    }

  

    async function sendMessage(message) {
        if (!botBaseUrl) {
            addMessage('Error: No se ha configurado la URL del bot.', false);
            return;
        }

        const formattedMessage = normalizeQuestion(message);
        addMessage(formattedMessage, true);
        $('#chatInput').val('');
        showTyping();
    
        try {
            const res = await fetch('/ChatProxy/PostToBot', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({ message: formattedMessage })
            });


            if (!res.ok) throw new Error(`Error HTTP: ${res.status}`);

            const data = await res.json();
            hideTyping();

            const texto = data.response || 'Sin respuesta.';
            const sugerencias = data.suggestions || [];
            const sinRespuesta = data.ticket_option || false;
            const titulo = data.titulo_sugerido || "";


            addMessage(texto, false, sugerencias, sinRespuesta, formattedMessage);
  
            
           
        } catch (err) {
            hideTyping();
            console.error('Fallo en fetch:', err);
            addMessage('Ocurrió un error al contactar al bot.', false);
        }
       
    }    

    function showTicketForm(titulo_sugerido = "") {
        const chatContainer = document.getElementById('chatContainer');
        formDiv = document.createElement('div');
        formDiv.className = 'ticket-form';
        formDiv.id = "ticketForm"; // le damos ID para manejarlo fácilmente

        formDiv.innerHTML = `
    <div class="card shadow p-4 mb-4">
        <h5 class="card-title mb-3">Crear Ticket</h5>  

        <div class="form-group mb-3">
            <input type="hidden" id="ticketTitle" name="ticketTitle" value="${titulo_sugerido}" />
        </div>

        <div class="form-group mb-4">
            <label for="ticketDesc" class="form-label">Descripción:</label>
            <textarea id="ticketDesc" class="form-control" rows="3" placeholder="Describa el problema o solicitud"></textarea>
        </div>

        <button class="btn btn-primary w-100" onclick="submitTicket()">Enviar Ticket</button>
    </div>
    `;

        chatContainer.appendChild(formDiv);
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }

    window.submitTicket = async function () {
        const nombre = window.usuarioId || '';
        const titulo = document.getElementById('ticketTitle').value.trim();
        const descripcion = document.getElementById('ticketDesc').value.trim();

        if (!nombre || !titulo || !descripcion) {
            alert('Por favor completa todos los campos.');
            return;
        }

        try {
            const res = await fetch('/ChatProxy/crear_ticket', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ nombre, titulo, descripcion })
            });

            const data = await res.json();

            const match = data.msg.match(/INCPH_\d+/);
            let codigo = match ? match[0] : "desconocido";

            let mes = "✅ Ticket enviado con éxito. N°: " + codigo;
            addMessage(mes, false);

            document.getElementById("ticketForm")?.remove();
            document.getElementById("btnCrearTicketChatbot")?.remove();

            addMessage('🔵 Deseas que te ayude con algo mas?', false);

        } catch {
            addMessage('⚠️Hubo un error al enviar el ticket. Contacte a Soporte Tecnico', false);
        }
    };

    // Evento Enter
    $input.keypress(function (e) {
        if (e.which === 13) {
            $sendBtn.click();
            return false;
        }
    });

    // Evento click en botón "Enviar"
    $sendBtn.click(function () {
        const txt = $input.val().trim();
        if (txt) {
            sendMessage(txt);
        }
    });

    
});
