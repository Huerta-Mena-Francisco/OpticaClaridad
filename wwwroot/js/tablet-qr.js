// tablet-qr.js - Script ultra-ligero para conexión tablet
class TabletConnector {
    constructor() {
        this.localIP = null;
        this.port = '5000';
        this.init();
    }

    async init() {
        await this.detectLocalIP();
        this.generateQR();
        this.setupEventListeners();
    }

    // Detectar IP local automáticamente
    async detectLocalIP() {
        try {
            // Intentar con WebRTC para IP local
            const peerConnection = new RTCPeerConnection({ iceServers: [] });
            peerConnection.createDataChannel('');
            peerConnection.createOffer()
                .then(offer => peerConnection.setLocalDescription(offer))
                .catch(() => { });

            peerConnection.onicecandidate = (event) => {
                if (event.candidate) {
                    const ipRegex = /([0-9]{1,3}(\.[0-9]{1,3}){3})/;
                    const match = ipRegex.exec(event.candidate.candidate);
                    if (match && !match[1].startsWith('0.')) {
                        this.localIP = match[1];
                        this.updateURL();
                        peerConnection.close();
                    }
                }
            };

            // Timeout después de 2 segundos
            setTimeout(() => {
                if (!this.localIP) {
                    this.localIP = '192.168.1.100'; // IP por defecto
                    this.updateURL();
                }
                peerConnection.close();
            }, 2000);
        } catch (error) {
            console.log('Error detectando IP:', error);
            this.localIP = window.location.hostname || 'localhost';
            this.updateURL();
        }
    }

    updateURL() {
        const url = `http://${this.localIP}:${this.port}`;
        document.getElementById('urlTablet').textContent = url;

        // Actualizar QR si existe
        if (typeof QRCode !== 'undefined') {
            document.getElementById('qrImage').style.display = 'block';
            document.getElementById('qrSpinner').style.display = 'none';
            new QRCode(document.getElementById('qrCodeImage'), {
                text: url,
                width: 200,
                height: 200,
                colorDark: "#000000",
                colorLight: "#ffffff",
                correctLevel: QRCode.CorrectLevel.H
            });
        }
    }

    generateQR() {
        // QR Code se generará cuando se tenga la IP
        console.log('QR listo para generarse cuando se detecte IP');
    }

    setupEventListeners() {
        // Copiar URL al portapapeles
        window.copiarUrl = () => {
            const url = document.getElementById('urlTablet').textContent;
            navigator.clipboard.writeText(url).then(() => {
                alert('✅ URL copiada al portapapeles: ' + url);
            }).catch(err => {
                console.error('Error copiando URL:', err);
                // Fallback para navegadores antiguos
                const textArea = document.createElement('textarea');
                textArea.value = url;
                document.body.appendChild(textArea);
                textArea.select();
                document.execCommand('copy');
                document.body.removeChild(textArea);
                alert('URL copiada');
            });
        };

        // Auto-abrir modal QR en tablets
        if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
            // Es un dispositivo móvil
            setTimeout(() => {
                const qrModal = new bootstrap.Modal(document.getElementById('qrModal'));
                qrModal.show();
            }, 3000);
        }
    }
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('qrModal')) {
        new TabletConnector();
    }
});