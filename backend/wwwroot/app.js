const { createApp } = Vue;
const TOKEN_KEY = "access_token";

axios.interceptors.request.use((config) => {
    const token = localStorage.getItem(TOKEN_KEY);
    if (token) {
        config.headers.Authorization = "Bearer " + token;
    }
    return config;
});

createApp({
    data() {
        return {
            items: [],
            loading: true,
            loginForm: {
                email: "",
                password: ""
            },
            newRecipe: {
                name: "",
                itemResult: "",
                count: 1,
                isShapeless: false,
                ingredients: {}
            },
            ingredientsInput: ""
        };
    },
    
    async mounted() {
        await this.loadItems();
    },

    methods: {
        async loadItems() {
            try {
                this.loading = true;
                const response = await axios.get("/recipes");
                this.items = response.data;
            } catch (error) {
                console.error("Помилка завантаження даних:", error);
            } finally {
                this.loading = false;
            }
        },
        
        async addItem() {
            try {
                const cleanIngredients = this.ingredientsInput.split(',')
                    .map(i => i.trim())
                    .filter(i => i.length > 0);

                const ingredientsDict = {};
                cleanIngredients.forEach((ing, index) => {
                    ingredientsDict[index.toString()] = ing;
                });

                this.newRecipe.ingredients = ingredientsDict;

                const response = await axios.post("/recipes", this.newRecipe);
                
                this.items.push(response.data);
                
                this.newRecipe = { name: "", itemResult: "", count: 1, isShapeless: false, ingredients: {} };
                this.ingredientsInput = "";
            } catch (error) {
                console.error("Не вдалося створити запис:", error);
            }
        },
        
        async updateItem(item) {
            try {
                await axios.put("/recipes/" + item.id, item);
                console.log(`Рецепт ${item.id} оновлено.`);
            } catch (error) {
                console.error("Помилка оновлення:", error);
            }
        },
        
        async deleteItem(id) {
            try {
                await axios.delete("/recipes/" + id);
                this.items = this.items.filter((item) => item.id !== id);
            } catch (error) {
                console.error("Помилка видалення:", error);
            }
        },
        
        async login() {
            try {
                const response = await axios.post("/auth/login", this.loginForm);
                localStorage.setItem(TOKEN_KEY, response.data.access_token);
                alert("Успішно авторизовано! Токен збережено.");

                this.loginForm.email = "";
                this.loginForm.password = "";
                await this.loadItems();
            } catch (error) {
                alert("Помилка входу! Перевірте логін та пароль.");
                console.error(error);
            }
        }
    }
}).mount("#app");