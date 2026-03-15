using OpenQA.Selenium;

namespace DoAnCuoiKy.Pages.Category
{
    public static class CategoryIndexLocators
    {
        public static readonly By PageTitle = By.CssSelector("h2.text-2xl");
        public static readonly By AddNewButton = By.CssSelector("button[onclick='showAddModal()']");
        public static readonly By CategoryTable = By.CssSelector("table.min-w-full");
        public static readonly By CategoryRows = By.CssSelector("tbody tr");
        public static readonly By CategoryModal = By.Id("categoryModal");
        public static readonly By ModalTitle = By.Id("modalTitle");
        public static readonly By CategoryIdInput = By.Id("categoryId");
        public static readonly By CategoryNameInput = By.Id("categoryName");
        public static readonly By FormSubmitButton = By.CssSelector("#categoryForm button[type='submit']");
    }
}
