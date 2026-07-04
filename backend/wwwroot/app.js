document.getElementById("login-form").addEventListener("submit", async (e) => {
    e.preventDefault();

    const res = await fetch("/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            email: document.getElementById("email").value,
            password: document.getElementById("password").value
        })
    });

    const data = await res.json();
    localStorage.setItem("access_token", data.access_token);
    alert("Успішно! Токен збережено.");
});

document.getElementById("load-btn").addEventListener("click", async () => {
    const token = localStorage.getItem("access_token");

    const res = await fetch("/recipes", {
        headers: { "Authorization": "Bearer " + token }
    });

    const recipes = await res.json();

    document.getElementById("data-list").innerHTML = recipes
        .map(r => {
            const ingredientsList = Object.values(r.ingredients).join(", ");

            return `
                <li>
                    <div class="recipe-name">${r.name}</div>
                    <div class="recipe-ingredients">${ingredientsList || "Без інгредієнтів"}</div>
                </li>
            `;
        })
        .join("");
});