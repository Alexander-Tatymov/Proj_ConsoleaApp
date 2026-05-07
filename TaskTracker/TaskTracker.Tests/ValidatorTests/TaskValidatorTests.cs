using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTracker.Core.Models;
using TaskTracker.Core.Validation;
namespace TaskTracker.Tests.ValidatorTests;
[TestClass]

public class TaskValidatorTests
{
    [TestMethod]
    public void Validate_TitleEmpty_ReturnsError()
    {
        var task = new TaskItem
        {
            Title = "",
            Description =
        ""
        };
        var error = TaskValidator.Validate(task);
        Assert.IsNotNull(error);
    }
    [TestMethod]
    public void Validate_TitleTooLong_ReturnsError()
    {
        var longTitle = new string('a', TaskValidator.TitleMaxLength + 1);
        var task = new TaskItem
        {
            Title = longTitle,
            Description = ""
        };
        var error = TaskValidator.Validate(task);
        Assert.IsNotNull(error);
    }
    [TestMethod]
    public void Validate_ValidTask_ReturnsNull()
    {
        var task = new TaskItem
        {
            Title = "Купить тетрадь",
            Description = "Описание",
            Status = Core.Models.TaskStatus.New
        };
        var error = TaskValidator.Validate(task);
        Assert.IsNull(error);
    
}
}