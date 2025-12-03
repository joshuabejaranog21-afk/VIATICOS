/**
 * ValidationMobile - Cliente JavaScript para la vista móvil
 * Maneja: conexión a sesión, captura de fotos, envío y recepción de resultados
 */
const ValidationMobile = (function () {
    'use strict';

    // Estado de la aplicación
    let state = {
        connection: null,
        sessionId: null,
        isConnected: false,
        currentImageBase64: null,
        isProcessing: false
    };

    // Elementos del DOM
    let elements = {};

    /**
     * Inicializa la aplicación móvil
     */
    function init(sessionId) {
        console.log('📱 Iniciando ValidationMobile con sesión:', sessionId);

        if (!sessionId) {
            showError('No se proporcionó ID de sesión');
            return;
        }

        state.sessionId = sessionId;

        // Cachear elementos del DOM
        cacheElements();

        // Configurar SignalR
        setupSignalR();

        // Event listeners
        setupEventListeners();
    }

    /**
     * Cachea referencias a elementos del DOM
     */
    function cacheElements() {
        elements = {
            headerSubtitle: document.getElementById('header-subtitle'),
            connectionStatus: document.getElementById('connection-status'),
            imageWrapper: document.getElementById('image-wrapper'),
            cameraPlaceholder: document.getElementById('camera-placeholder'),
            previewImage: document.getElementById('preview-image'),
            fileInput: document.getElementById('file-input'),
            btnTakePhoto: document.getElementById('btn-take-photo'),
            btnValidate: document.getElementById('btn-validate'),
            btnRetry: document.getElementById('btn-retry'),
            resultContainer: document.getElementById('result-container'),
            processingOverlay: document.getElementById('processing-overlay'),
            sessionIdDisplay: document.getElementById('session-id-display')
        };
    }

    /**
     * Configura la conexión de SignalR
     */
    function setupSignalR() {
        console.log('🔌 Configurando SignalR...');

        state.connection = new signalR.HubConnectionBuilder()
            .withUrl('/validationHub')
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Eventos de conexión
        state.connection.onreconnecting(() => {
            console.warn('⚠️ Reconectando SignalR...');
            updateConnectionStatus('connecting', '⏳ Reconectando...');
        });

        state.connection.onreconnected(() => {
            console.log('✅ SignalR reconectado');
            updateConnectionStatus('connected', '✅ Conectado');
            joinSession();
        });

        state.connection.onclose(() => {
            console.error('❌ Conexión SignalR cerrada');
            state.isConnected = false;
            updateConnectionStatus('error', '❌ Desconectado');
        });

        // Handlers de eventos del Hub
        setupSignalRHandlers();

        // Iniciar conexión
        startSignalRConnection();
    }

    /**
     * Configura los handlers de eventos de SignalR
     */
    function setupSignalRHandlers() {
        // Unido exitosamente a la sesión
        state.connection.on('JoinedSession', (data) => {
            console.log('✅ Unido a sesión:', data);
            state.isConnected = true;
            updateConnectionStatus('connected', '✅ Conectado');
            elements.headerSubtitle.textContent = 'Conectado. Listo para tomar fotos.';
        });

        // Imagen enviada (confirmación)
        state.connection.on('ImageSent', (data) => {
            console.log('📤 Imagen enviada:', data);
        });

        // Resultado de validación
        state.connection.on('ValidationResult', (result) => {
            console.log('📊 Resultado recibido:', result);
            hideProcessing();
            displayResult(result);
        });

        // Error
        state.connection.on('Error', (error) => {
            console.error('❌ Error del Hub:', error);
            hideProcessing();
            showError(error.message);
            updateConnectionStatus('error', '❌ Error');
        });

        // Admin desconectado
        state.connection.on('AdminDisconnected', (data) => {
            console.log('💻 Admin desconectado:', data);
            showError('El administrador se desconectó. La sesión puede no estar disponible.');
        });

        // Sesión cerrada
        state.connection.on('SessionClosed', (data) => {
            console.log('🔒 Sesión cerrada:', data);
            showError('La sesión ha sido cerrada. Por favor, escanee un nuevo código QR.');
            disableInterface();
        });
    }

    /**
     * Inicia la conexión de SignalR
     */
    async function startSignalRConnection() {
        try {
            await state.connection.start();
            console.log('✅ SignalR conectado');
            elements.headerSubtitle.textContent = 'Conectado al servidor. Uniéndose a sesión...';

            // Unirse a la sesión
            await joinSession();

        } catch (err) {
            console.error('❌ Error al conectar SignalR:', err);
            updateConnectionStatus('error', '❌ Error de conexión');
            showError('No se pudo conectar al servidor. Verifique su conexión.');
            setTimeout(startSignalRConnection, 5000);
        }
    }

    /**
     * Se une a la sesión especificada
     */
    async function joinSession() {
        try {
            console.log('🔗 Uniéndose a sesión:', state.sessionId);
            await state.connection.invoke('JoinSession', state.sessionId);
        } catch (err) {
            console.error('❌ Error al unirse a sesión:', err);
            showError('No se pudo unir a la sesión. El código QR puede haber expirado.');
        }
    }

    /**
     * Configura event listeners
     */
    function setupEventListeners() {
        // Botón tomar foto
        elements.btnTakePhoto?.addEventListener('click', () => {
            elements.fileInput.click();
        });

        // Input de archivo (cuando se selecciona una imagen)
        elements.fileInput?.addEventListener('change', handleFileSelect);

        // Botón validar
        elements.btnValidate?.addEventListener('click', sendImageForValidation);

        // Botón reintentar
        elements.btnRetry?.addEventListener('click', resetCapture);
    }

    /**
     * Maneja la selección de archivo/foto
     */
    function handleFileSelect(event) {
        const file = event.target.files[0];

        if (!file) {
            return;
        }

        if (!file.type.startsWith('image/')) {
            showError('Por favor, seleccione una imagen válida');
            return;
        }

        console.log('📸 Foto seleccionada:', file.name, file.type);

        // Leer la imagen como Base64
        const reader = new FileReader();

        reader.onload = (e) => {
            const base64Image = e.target.result;
            state.currentImageBase64 = base64Image;

            // Mostrar preview
            elements.previewImage.src = base64Image;
            elements.previewImage.style.display = 'block';
            elements.cameraPlaceholder.style.display = 'none';

            // Mostrar botones de validar y reintentar
            elements.btnTakePhoto.style.display = 'none';
            elements.btnValidate.style.display = 'block';
            elements.btnRetry.style.display = 'block';

            // Limpiar resultado anterior
            elements.resultContainer.innerHTML = '';
        };

        reader.onerror = () => {
            showError('Error al leer la imagen');
        };

        reader.readAsDataURL(file);
    }

    /**
     * Envía la imagen para validación
     */
    async function sendImageForValidation() {
        if (!state.currentImageBase64) {
            showError('No hay imagen para validar');
            return;
        }

        if (state.isProcessing) {
            return;
        }

        state.isProcessing = true;
        showProcessing();

        try {
            console.log('📤 Enviando imagen para validación...');

            // Enviar vía API HTTP
            const response = await fetch('/api/validation/analyze', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    sessionId: state.sessionId,
                    imageBase64: state.currentImageBase64,
                    description: null,
                    clientTimestamp: new Date().toISOString()
                })
            });

            if (!response.ok) {
                throw new Error('Error al procesar la imagen');
            }

            console.log('✅ Imagen enviada exitosamente');

            // El resultado llegará vía SignalR (evento ValidationResult)

        } catch (error) {
            console.error('❌ Error al enviar imagen:', error);
            hideProcessing();
            showError('Error al enviar la imagen. Por favor, intente nuevamente.');
            state.isProcessing = false;
        }
    }

    /**
     * Muestra el resultado de la validación
     */
    function displayResult(result) {
        state.isProcessing = false;

        const resultClass = result.isDeductible ? 'deductible' : 'non-deductible';
        const resultIcon = result.isDeductible ? '✅' : '❌';
        const resultTitle = result.isDeductible ? 'DEDUCIBLE' : 'NO DEDUCIBLE';
        const confidencePercent = (result.confidence * 100).toFixed(0);

        const resultHtml = `
            <div class="result-mobile ${resultClass}">
                <h2>${resultIcon}</h2>
                <h3>${resultTitle}</h3>

                <p style="font-size: 18px; margin: 16px 0; font-weight: 600;">
                    ${result.productName}
                </p>

                <p style="font-size: 14px; opacity: 0.9;">
                    <strong>Categoría:</strong> ${result.category}
                </p>

                <p style="font-size: 14px; opacity: 0.9; margin-top: 12px;">
                    ${result.reason}
                </p>

                <div style="margin-top: 16px;">
                    <p style="font-size: 12px; margin-bottom: 8px;">
                        <strong>Confianza: ${confidencePercent}%</strong>
                    </p>
                    <div class="confidence-bar">
                        <div class="confidence-fill" style="width: ${confidencePercent}%"></div>
                    </div>
                </div>

                ${result.requiresManualReview ? `
                    <p style="margin-top: 16px; font-size: 14px; opacity: 0.9;">
                        ⚠️ Requiere revisión manual
                    </p>
                ` : ''}
            </div>
        `;

        elements.resultContainer.innerHTML = resultHtml;

        // Permitir tomar otra foto
        elements.btnValidate.style.display = 'none';
        elements.btnRetry.textContent = '🔄 Validar Otro Producto';
    }

    /**
     * Resetea la captura para tomar otra foto
     */
    function resetCapture() {
        // Limpiar estado
        state.currentImageBase64 = null;

        // Resetear UI
        elements.previewImage.style.display = 'none';
        elements.previewImage.src = '';
        elements.cameraPlaceholder.style.display = 'flex';

        elements.btnTakePhoto.style.display = 'block';
        elements.btnValidate.style.display = 'none';
        elements.btnRetry.style.display = 'none';

        elements.resultContainer.innerHTML = '';

        // Resetear input de archivo
        elements.fileInput.value = '';
    }

    /**
     * Actualiza el estado de conexión visual
     */
    function updateConnectionStatus(status, text) {
        elements.connectionStatus.className = `connection-badge ${status}`;
        elements.connectionStatus.textContent = text;
    }

    /**
     * Muestra el overlay de procesamiento
     */
    function showProcessing() {
        elements.processingOverlay?.classList.remove('hidden');
    }

    /**
     * Oculta el overlay de procesamiento
     */
    function hideProcessing() {
        elements.processingOverlay?.classList.add('hidden');
    }

    /**
     * Muestra un mensaje de error
     */
    function showError(message) {
        alert(`❌ Error: ${message}`);
    }

    /**
     * Deshabilita la interfaz (cuando la sesión se cierra)
     */
    function disableInterface() {
        elements.btnTakePhoto.disabled = true;
        elements.btnValidate.disabled = true;
        elements.btnRetry.disabled = true;
    }

    // API pública
    return {
        init
    };
})();

// No auto-inicializar, esperar a que la vista llame a init()
