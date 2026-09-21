// Choose a physical camera before MindAR initializes its video dimensions.
window.PUCmonCamera = {
    async open() {
        const storageKey = "pucmon.camera";
        let deviceId;
        try { deviceId = sessionStorage.getItem(storageKey); } catch (_) {}

        const media = navigator.mediaDevices;
        let stream;
        try {
            stream = await media.getUserMedia({
                audio: false,
                video: deviceId ? { deviceId: { exact: deviceId } } : { facingMode: "environment" }
            });
        } catch (error) {
            if (!deviceId || !["OverconstrainedError", "NotFoundError"].includes(error.name)) throw error;
            try { sessionStorage.removeItem(storageKey); } catch (_) {}
            stream = await media.getUserMedia({ audio: false, video: { facingMode: "environment" } });
        }

        // Labels become available after the user grants camera access.
        try {
            const cameras = (await media.enumerateDevices()).filter(device => device.kind === "videoinput");
            document.getElementById("pucmon-camera-picker")?.remove();
            if (cameras.length < 2) return stream;

            const label = document.createElement("label");
            label.id = "pucmon-camera-picker";
            label.textContent = "Câmera ";
            label.style.cssText = "position:fixed;top:max(12px,env(safe-area-inset-top));left:12px;z-index:10000;max-width:calc(100vw - 80px);padding:8px;border-radius:8px;background:#ffffffed;color:#111;font:14px sans-serif";
            const select = document.createElement("select");
            select.style.cssText = "max-width:100%;font:inherit;color:#111;background:white";
            select.setAttribute("aria-label", "Escolher câmera");
            const activeId = stream.getVideoTracks()[0].getSettings().deviceId;
            cameras.forEach((camera, index) => {
                const option = document.createElement("option");
                option.value = camera.deviceId;
                option.textContent = camera.label || `Câmera ${index + 1}`;
                option.selected = camera.deviceId === activeId;
                select.appendChild(option);
            });
            select.addEventListener("change", () => {
                try {
                    sessionStorage.setItem(storageKey, select.value);
                } catch (_) {
                    select.value = activeId;
                    alert("Não foi possível salvar a escolha da câmera neste navegador.");
                    return;
                }
                // Restart both the tracker and Unity with the new camera dimensions.
                stream.getTracks().forEach(track => track.stop());
                location.reload();
            });
            label.appendChild(select);
            document.body.appendChild(label);
        } catch (error) {
            console.warn("Não foi possível listar as câmeras disponíveis.", error);
        }
        return stream;
    }
};
