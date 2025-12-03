/**
 * ValidationAdmin - Cliente JavaScript para la vista de administración
 * Maneja: creación de sesión, QR, SignalR, recepción de imágenes y resultados
 */
const ValidationAdmin = (function () {
    'use strict';

    // Estado de la aplicación
    let state = {
        connection: null,
        sessionId: null,
        isConnected: false,
        mobileConnected: false,
        stats: {
            total: 0,
            deductible: 0,
            nonDeductible: 0
        },
        history: []
    };

    // Elementos del DOM
    let elements = {};

    /**
     * Inicializa la aplicación
     */
    function init() {
        console.log('🚀 Iniciando ValidationAdmin...');

        // Cachear elementos del DOM
        cacheElements();

        // Configurar SignalR
        setupSignalR();

        // Crear sesión inicial
        createSession();

        // Event listeners
        setupEventListeners();
    }

    /**
     * Cachea referencias a elementos del DOM
     */
    function cacheElements() {
        elements = {
            qrContainer: document.getElementById('qr-container'),
            qrInfo: document.getElementById('qr-info'),
            btnNewSession: document.getElementById('btn-new-session'),
            connectionStatus: document.getElementById('connection-status'),
            statusTitle: document.getElementById('status-title'),
            statusMessage: document.getElementById('status-message'),
            sessionStats: document.getElementById('session-stats'),
            statCount: document.getElementById('stat-count'),
            statDeductible: document.getElementById('stat-deductible'),
            statNonDeductible: document.getElementById('stat-non-deductible'),
            imagePreviewSection: document.getElementById('image-preview-section'),
            previewImage: document.getElementById('preview-image'),
            previewLoading: document.getElementById('preview-loading'),
            resultSection: document.getElementById('result-section'),
            resultCard: document.getElementById('result-card'),
            historySection: document.getElementById('history-section'),
            historyList: document.getElementById('history-list'),
            sessionIdInput: document.getElementById('session-id')
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
            showToast('Reconectando...', 'warning');
        });

        state.connection.onreconnected(() => {
            console.log('✅ SignalR reconectado');
            showToast('Reconectado exitosamente', 'success');

            // Re-crear sesión si existía una
            if (state.sessionId) {
                state.connection.invoke('CreateSession', state.sessionId)
                    .catch(err => console.error('Error al re-crear sesión:', err));
            }
        });

        state.connection.onclose(() => {
            console.error('❌ Conexión SignalR cerrada');
            state.isConnected = false;
            showToast('Conexión perdida. Recargue la página.', 'error');
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
        // Sesión creada exitosamente
        state.connection.on('SessionCreated', (data) => {
            console.log('✅ Sesión creada en el Hub:', data);
            showToast('Sesión creada exitosamente', 'success');
        });

        // Móvil se conectó
        state.connection.on('MobileConnected', (data) => {
            console.log('📱 Móvil conectado:', data);
            state.mobileConnected = true;
            updateConnectionStatus('connected', 'Teléfono Conectado ✓', data.message);
            showToast('¡Teléfono móvil conectado!', 'success');
            elements.sessionStats.classList.remove('hidden');
        });

        // Móvil se desconectó
        state.connection.on('MobileDisconnected', (data) => {
            console.log('📱 Móvil desconectado:', data);
            state.mobileConnected = false;
            updateConnectionStatus('waiting', 'Móvil Desconectado', data.message);
            showToast('Teléfono desconectado', 'warning');
        });

        // Imagen recibida del móvil
        state.connection.on('ImageReceived', (data) => {
            console.log('📸 Imagen recibida:', data);
            displayReceivedImage(data.imageBase64, data.description);
            showToast('Imagen recibida, analizando...', 'info');
        });

        // Resultado de validación
        state.connection.on('ValidationResult', (result) => {
            console.log('📊 Resultado recibido:', result);
            displayValidationResult(result);
            updateStats(result.isDeductible);
            addToHistory(result);
            showToast(`Análisis completado: ${result.productName}`, 'success');
        });

        // Error
        state.connection.on('Error', (error) => {
            console.error('❌ Error del Hub:', error);
            showToast(`Error: ${error.message}`, 'error');
        });

        // Sesión cerrada
        state.connection.on('SessionClosed', (data) => {
            console.log('🔒 Sesión cerrada:', data);
            showToast('Sesión cerrada', 'info');
        });
    }

    /**
     * Inicia la conexión de SignalR
     */
    async function startSignalRConnection() {
        try {
            await state.connection.start();
            console.log('✅ SignalR conectado');
            state.isConnected = true;
        } catch (err) {
            console.error('❌ Error al conectar SignalR:', err);
            showToast('Error al conectar con el servidor', 'error');
            setTimeout(startSignalRConnection, 5000);
        }
    }

    /**
     * Crea una nueva sesión de validación
     */
    async function createSession() {
        try {
            console.log('🆕 Creando nueva sesión...');

            const response = await fetch('/api/validation/session/create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Error al crear sesión');
            }

            const session = await response.json();
            console.log('✅ Sesión creada:', session);

            state.sessionId = session.sessionId;
            elements.sessionIdInput.value = session.sessionId;

            // Mostrar QR
            displayQRCode(session.qrCodeBase64);

            // Notificar al Hub que la sesión fue creada
            await state.connection.invoke('CreateSession', session.sessionId);

            showToast('Sesión creada. Escanee el QR con su teléfono.', 'success');

        } catch (error) {
            console.error('❌ Error al crear sesión:', error);
            showToast('Error al crear sesión', 'error');
        }
    }

    /**
     * Muestra el código QR en pantalla
     */
    function displayQRCode(base64Image) {
        elements.qrContainer.innerHTML = `
            <img src="data:image/png;base64,${base64Image}"
                 alt="Código QR"
                 class="qr-image" />
        `;

        elements.qrInfo.classList.remove('hidden');
        elements.btnNewSession.classList.remove('hidden');
    }

    /**
     * Muestra la imagen recibida del móvil
     */
    function displayReceivedImage(base64Image, description) {
        const imageSrc = base64Image.startsWith('data:')
            ? base64Image
            : `data:image/jpeg;base64,${base64Image}`;

        elements.previewImage.src = imageSrc;
        elements.previewImage.style.display = 'block';
        elements.previewLoading.style.display = 'flex';

        elements.imagePreviewSection.classList.remove('hidden');
        elements.resultSection.classList.add('hidden');

        updateConnectionStatus('processing', 'Procesando Imagen...', 'Analizando con Claude AI...');
    }

    /**
     * Muestra el resultado de la validación
     */
    function displayValidationResult(result) {
        elements.previewLoading.style.display = 'none';

        const statusClass = result.isDeductible ? 'deductible' : 'non-deductible';
        const statusIcon = result.isDeductible ? '✅' : '❌';
        const statusText = result.isDeductible ? 'DEDUCIBLE' : 'NO DEDUCIBLE';
        const confidencePercent = (result.confidence * 100).toFixed(0);

        elements.resultCard.className = `result-card ${statusClass}`;
        elements.resultCard.innerHTML = `
            <div class="result-header">
                <div class="result-icon">${statusIcon}</div>
                <div class="result-status">${statusText}</div>
            </div>

            <div class="result-body">
                <h3 class="result-product-name">${result.productName}</h3>

                <div class="result-detail">
                    <span class="result-label">Categoría:</span>
                    <span class="result-value">${result.category}</span>
                </div>

                <div class="result-detail">
                    <span class="result-label">Confianza:</span>
                    <span class="result-value">${confidencePercent}%</span>
                    <div class="confidence-bar">
                        <div class="confidence-fill" style="width: ${confidencePercent}%"></div>
                    </div>
                </div>

                <div class="result-reason">
                    <strong>Razón:</strong>
                    <p>${result.reason}</p>
                </div>

                ${result.requiresManualReview ? `
                    <div class="result-warning">
                        ⚠️ Este producto requiere revisión manual adicional
                    </div>
                ` : ''}

                ${result.additionalNotes ? `
                    <div class="result-notes">
                        <small>${result.additionalNotes}</small>
                    </div>
                ` : ''}
            </div>

            <div class="result-footer">
                <small>Método: ${result.analysisMethod}</small>
                <small>${new Date(result.timestamp).toLocaleTimeString('es-MX')}</small>
            </div>
        `;

        elements.resultSection.classList.remove('hidden');

        updateConnectionStatus('connected', 'Análisis Completado', 'Listo para siguiente producto');
    }

    /**
     * Actualiza las estadísticas de la sesión
     */
    function updateStats(isDeductible) {
        state.stats.total++;

        if (isDeductible) {
            state.stats.deductible++;
        } else {
            state.stats.nonDeductible++;
        }

        elements.statCount.textContent = state.stats.total;
        elements.statDeductible.textContent = state.stats.deductible;
        elements.statNonDeductible.textContent = state.stats.nonDeductible;
    }

    /**
     * Agrega un item al historial
     */
    function addToHistory(result) {
        state.history.unshift(result);

        const historyItem = document.createElement('div');
        historyItem.className = `history-item ${result.isDeductible ? 'deductible' : 'non-deductible'}`;
        historyItem.innerHTML = `
            <div class="history-icon">${result.isDeductible ? '✅' : '❌'}</div>
            <div class="history-content">
                <strong>${result.productName}</strong>
                <small>${result.category}</small>
            </div>
            <div class="history-time">
                ${new Date(result.timestamp).toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' })}
            </div>
        `;

        elements.historyList.insertBefore(historyItem, elements.historyList.firstChild);
        elements.historySection.classList.remove('hidden');
    }

    /**
     * Actualiza el estado de conexión visual
     */
    function updateConnectionStatus(status, title, message) {
        elements.connectionStatus.className = `connection-status status-${status}`;
        elements.statusTitle.textContent = title;
        elements.statusMessage.textContent = message;

        const icons = {
            waiting: '⏳',
            connected: '✅',
            processing: '⚙️',
            error: '❌'
        };

        const iconElement = elements.connectionStatus.querySelector('.status-icon');
        if (iconElement) {
            iconElement.textContent = icons[status] || '❓';
        }
    }

    /**
     * Muestra un toast de notificación
     */
    function showToast(message, type = 'info') {
        const container = document.getElementById('toast-container') || createToastContainer();

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;

        const icons = {
            success: '✅',
            error: '❌',
            warning: '⚠️',
            info: 'ℹ️'
        };

        toast.innerHTML = `
            <span class="toast-icon">${icons[type] || 'ℹ️'}</span>
            <span class="toast-message">${message}</span>
        `;

        container.appendChild(toast);

        // Auto-remover después de 4 segundos
        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease forwards';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }

    /**
     * Crea el contenedor de toasts si no existe
     */
    function createToastContainer() {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container';
        document.body.appendChild(container);
        return container;
    }

    /**
     * Configura event listeners
     */
    function setupEventListeners() {
        // Botón de nueva sesión
        elements.btnNewSession?.addEventListener('click', () => {
            if (confirm('¿Está seguro de que desea crear una nueva sesión? La sesión actual se cerrará.')) {
                // Cerrar sesión actual
                if (state.sessionId) {
                    state.connection.invoke('CloseSession', state.sessionId)
                        .catch(err => console.error('Error al cerrar sesión:', err));
                }

                // Resetear estado
                state.stats = { total: 0, deductible: 0, nonDeductible: 0 };
                state.history = [];
                elements.historyList.innerHTML = '';
                elements.historySection.classList.add('hidden');
                elements.imagePreviewSection.classList.add('hidden');
                elements.resultSection.classList.add('hidden');

                // Crear nueva sesión
                createSession();
            }
        });
    }

    // API pública
    return {
        init
    };
})();

// No auto-inicializar, esperar a que la vista llame a init()
