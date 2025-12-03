/**
 * Ticket Analyzer - Analiza recibos con Claude AI y permite edición manual
 */
const TicketAnalyzer = (function () {
    'use strict';

    // Estado de la aplicación
    let state = {
        currentReceipt: null,
        products: [],
        stats: {
            total: 0,
            deductible: 0,
            nonDeductible: 0
        }
    };

    // Elementos del DOM
    let elements = {};

    /**
     * Inicializa la aplicación
     */
    function init() {
        console.log('Iniciando Ticket Analyzer...');

        // Cachear elementos del DOM
        cacheElements();

        // Event listeners
        setupEventListeners();
    }

    /**
     * Cachea referencias a elementos del DOM
     */
    function cacheElements() {
        elements = {
            receiptImage: document.getElementById('receiptImage'),
            btnAnalyzeReceipt: document.getElementById('btnAnalyzeReceipt'),
            loadingSpinner: document.getElementById('loadingSpinner'),
            resultsSection: document.getElementById('resultsSection'),
            vendorName: document.getElementById('vendorName'),
            ticketDate: document.getElementById('ticketDate'),
            totalAmount: document.getElementById('totalAmount'),
            statTotalProducts: document.getElementById('statTotalProducts'),
            statDeductible: document.getElementById('statDeductible'),
            statNonDeductible: document.getElementById('statNonDeductible'),
            productsTableBody: document.getElementById('productsTableBody')
        };
    }

    /**
     * Configura event listeners
     */
    function setupEventListeners() {
        // Botón de analizar recibo
        elements.btnAnalyzeReceipt.addEventListener('click', analyzeReceipt);

        // Cambio de archivo
        elements.receiptImage.addEventListener('change', function () {
            if (this.files && this.files[0]) {
                elements.btnAnalyzeReceipt.disabled = false;
            }
        });
    }

    /**
     * Analiza el recibo con Claude AI
     */
    async function analyzeReceipt() {
        const file = elements.receiptImage.files[0];

        if (!file) {
            alert('Por favor selecciona una imagen del recibo');
            return;
        }

        // Mostrar loading
        elements.btnAnalyzeReceipt.disabled = true;
        elements.loadingSpinner.classList.remove('d-none');
        elements.resultsSection.classList.add('d-none');

        try {
            // Crear FormData
            const formData = new FormData();
            formData.append('image', file);

            // Llamar al endpoint
            const response = await fetch('/api/ocr/process-with-claude', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error('Error al procesar el recibo');
            }

            const result = await response.json();

            if (result.success) {
                // Guardar datos
                state.currentReceipt = result;
                state.products = result.products || [];

                // Mostrar resultados
                displayResults(result);

                console.log('Recibo procesado exitosamente:', result);
            } else {
                alert('Error: ' + result.message);
            }
        } catch (error) {
            console.error('Error al analizar recibo:', error);
            alert('Error al procesar el recibo. Por favor intenta de nuevo.');
        } finally {
            // Ocultar loading
            elements.loadingSpinner.classList.add('d-none');
            elements.btnAnalyzeReceipt.disabled = false;
        }
    }

    /**
     * Muestra los resultados del análisis
     */
    function displayResults(result) {
        // Información del recibo
        elements.vendorName.textContent = result.vendor || 'No detectado';
        elements.ticketDate.textContent = result.ticketDate || 'No detectada';
        elements.totalAmount.textContent = result.totalAmount
            ? `$${result.totalAmount.toFixed(2)}`
            : 'No detectado';

        // Calcular estadísticas
        calculateStats();

        // Mostrar productos
        displayProducts();

        // Mostrar sección de resultados
        elements.resultsSection.classList.remove('d-none');
    }

    /**
     * Calcula las estadísticas de los productos
     */
    function calculateStats() {
        state.stats.total = state.products.length;
        state.stats.deductible = state.products.filter(p => p.isDeductible).length;
        state.stats.nonDeductible = state.products.filter(p => !p.isDeductible).length;

        // Actualizar DOM
        elements.statTotalProducts.textContent = state.stats.total;
        elements.statDeductible.textContent = state.stats.deductible;
        elements.statNonDeductible.textContent = state.stats.nonDeductible;
    }

    /**
     * Muestra los productos en la tabla
     */
    function displayProducts() {
        elements.productsTableBody.innerHTML = '';

        state.products.forEach((product, index) => {
            const row = createProductRow(product, index);
            elements.productsTableBody.appendChild(row);
        });
    }

    /**
     * Crea una fila de producto
     */
    function createProductRow(product, index) {
        const row = document.createElement('tr');
        row.id = `product-row-${product.id}`;

        if (product.manuallyOverridden) {
            row.classList.add('product-row-modified');
        }

        const statusBadge = product.isDeductible
            ? '<span class="badge badge-deductible">Deducible</span>'
            : '<span class="badge badge-non-deductible">No Deducible</span>';

        const modifiedBadge = product.manuallyOverridden
            ? ' <span class="badge badge-modified">Modificado</span>'
            : '';

        row.innerHTML = `
            <td>${index + 1}</td>
            <td>
                <strong>${product.name}</strong>
                <br>
                <small class="text-muted">${product.reason}</small>
            </td>
            <td>${product.quantity}</td>
            <td>$${product.unitPrice.toFixed(2)}</td>
            <td>$${product.totalPrice.toFixed(2)}</td>
            <td>
                <span class="badge bg-secondary">${product.category}</span>
            </td>
            <td>
                ${statusBadge}${modifiedBadge}
                <br>
                <small class="text-muted">Confianza: ${(product.confidence * 100).toFixed(0)}%</small>
            </td>
            <td>
                <div class="btn-group btn-group-sm" role="group">
                    <button type="button"
                            class="btn btn-deductible ${product.isDeductible && !product.manuallyOverridden ? 'active' : ''}"
                            onclick="TicketAnalyzer.markAsDeductible('${product.id}')"
                            ${product.isDeductible && !product.manuallyOverridden ? 'disabled' : ''}>
                        <i class="bi bi-check-circle"></i> Deducible
                    </button>
                    <button type="button"
                            class="btn btn-non-deductible ${!product.isDeductible && !product.manuallyOverridden ? 'active' : ''}"
                            onclick="TicketAnalyzer.markAsNonDeductible('${product.id}')"
                            ${!product.isDeductible && !product.manuallyOverridden ? 'disabled' : ''}>
                        <i class="bi bi-x-circle"></i> No Deducible
                    </button>
                </div>
            </td>
        `;

        return row;
    }

    /**
     * Marca un producto como deducible
     */
    async function markAsDeductible(productId) {
        await updateProductDeductibility(productId, true);
    }

    /**
     * Marca un producto como no deducible
     */
    async function markAsNonDeductible(productId) {
        await updateProductDeductibility(productId, false);
    }

    /**
     * Actualiza el estado de deducibilidad de un producto
     */
    async function updateProductDeductibility(productId, isDeductible) {
        try {
            // Llamar al endpoint
            const response = await fetch('/api/ocr/update-product-deductibility', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    productId: productId,
                    isDeductible: isDeductible,
                    reason: 'Modificado manualmente por el usuario'
                })
            });

            if (!response.ok) {
                throw new Error('Error al actualizar el producto');
            }

            const result = await response.json();

            if (result.success) {
                // Actualizar el producto en el estado
                const product = state.products.find(p => p.id === productId);
                if (product) {
                    product.isDeductible = isDeductible;
                    product.manuallyOverridden = true;
                    product.reason = 'Modificado manualmente por el usuario';

                    // Recalcular estadísticas
                    calculateStats();

                    // Re-renderizar la fila
                    const productIndex = state.products.findIndex(p => p.id === productId);
                    const row = createProductRow(product, productIndex);
                    const oldRow = document.getElementById(`product-row-${productId}`);
                    if (oldRow) {
                        oldRow.replaceWith(row);
                    }

                    console.log(`Producto ${productId} actualizado a ${isDeductible ? 'deducible' : 'no deducible'}`);
                }
            } else {
                alert('Error al actualizar: ' + result.message);
            }
        } catch (error) {
            console.error('Error al actualizar producto:', error);
            alert('Error al actualizar el producto. Por favor intenta de nuevo.');
        }
    }

    // API pública
    return {
        init,
        markAsDeductible,
        markAsNonDeductible
    };
})();

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function () {
    TicketAnalyzer.init();
});
