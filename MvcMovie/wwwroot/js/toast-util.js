// Toast utility
function showToast(message, type) {
    const toast = document.getElementById("toast");
    if (!toast) return;
    toast.textContent = message;
    toast.className = `toast ${type}`;
    toast.style.display = "block";
    setTimeout(() => {
        toast.style.display = "none";
    }, 3000);
}

// AJAX form handler for create/edit
function bindAjaxForm(formSelector, object, onSuccess) {
    const form = document.querySelector(formSelector);
    const toast = document.getElementById("toast");
    if (!form) return;

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        try {
            const response = await fetch(event.target.action, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body: new URLSearchParams(new FormData(event.target)).toString()
            });

            if (response.ok) {
                const data = await response.json();
                if (typeof onSuccess === "function") {
                    onSuccess(data);
                }
            } else {
                const errorData = await response.json();
                showToast(`Failed to save ${object}: ${errorData.error}`, "error");
            }
        } catch (error) {
            showToast(`Error saving ${object}: ${error.message}`, "error");
        }
    });
}