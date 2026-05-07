using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTracker.Core.Services;
namespace TaskTracker.Tests.ServiceTests;
[TestClass]
public class TaskServiceTests
{
    [TestMethod]
    public void Add_ValidTitle_TaskAddedWithId1()
    {
    
var service = new TaskService();
        var task = service.Add("Test task");
        Assert.AreEqual(1, task.Id);
        Assert.AreEqual("Test task", task.Title);
        Assert.AreEqual(1, service.GetAll().Count);
    }
    [TestMethod]
    public void Add_EmptyTitle_Throws()
    {
        var service = new TaskService();
        Assert.ThrowsException<ArgumentException>(() => service.Add(""));
    }
    [TestMethod]
    public void Delete_ExistingId_RemovesTask()
    {
        var service = new TaskService();
        var task = service.Add("To delete");
        service.Delete(task.Id);
        Assert.AreEqual(0, service.GetAll().Count);
    }
    [TestMethod]
    public void Update_ChangesTitleAndDescription()
    {
        var service = new TaskService();
        var task = service.Add("Old");
        var updated = service.Update(task.Id, "New", "Desc");
        Assert.AreEqual("New", updated.Title);
    
Assert.AreEqual("Desc", updated.Description);
        Assert.AreEqual(1, task.Id);
    }
}