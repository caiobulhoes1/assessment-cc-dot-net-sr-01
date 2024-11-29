using System.Diagnostics;
using API.Controllers;
using API.Test.Fixtures;
using FluentAssertions;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Test;

public class MonsterExtendedControllerTests
{
    private readonly Mock<IBattleOfMonstersRepository> _repository;
    private readonly MonsterController _monsterController;

    public MonsterExtendedControllerTests()
    {
        _repository = new Mock<IBattleOfMonstersRepository>();
        _monsterController = new MonsterController(_repository.Object);
    }

    [Fact]
    public async Task Post_OnSuccess_ImportCsvToMonster()
    {
        var directory = Path.Combine("Files");
        var filePath = Path.Combine(directory, "monsters-correct.csv");

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var formFile = new FormFile(new FileStream(filePath, FileMode.Open), 0, new FileInfo(filePath).Length, "file", Path.GetFileName(filePath));

        _repository.Setup(r => r.Monsters.AddAsync(It.IsAny<IEnumerable<Monster>>())).Returns(Task.CompletedTask);
        _repository.Setup(r => r.Save());

        var result = await _monsterController.ImportCsv(formFile);
        var okResult = Assert.IsType<OkResult>(result);
        Assert.NotNull(okResult);

        _repository.Verify(r => r.Monsters.AddAsync(It.IsAny<IEnumerable<Monster>>()), Times.Once);
        _repository.Verify(r => r.Save(),Times.Once);
    }

    [Fact]
    public async Task Post_BadRequest_ImportCsv_With_Nonexistent_Monster()
    {
        var directory = Path.Combine("Files");
        var filePath = Path.Combine(directory, "monsters-empty-monster.csv");

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var formFile = new FormFile(new FileStream(filePath, FileMode.Open), 0, new FileInfo(filePath).Length, "file", Path.GetFileName(filePath));

        _repository.Setup(r => r.Monsters.AddAsync(It.IsAny<IEnumerable<Monster>>())).Returns(Task.CompletedTask);
        _repository.Setup(r => r.Save());
        _repository.Setup(r => r.Monsters.FindAsync(It.IsAny<int?>())).ReturnsAsync((Monster)null);

        var result = await _monsterController.ImportCsv(formFile);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        Assert.Equal("Wrong data mapping.", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_BadRequest_ImportCsv_With_Nonexistent_Column()
    {
        var directory = Path.Combine("Files");
        var filePath = Path.Combine(directory, "monsters-wrong-column.csv");

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var formFile = new FormFile(new FileStream(filePath, FileMode.Open), 0, new FileInfo(filePath).Length, "file", Path.GetFileName(filePath));

        _repository.Setup(r => r.Monsters.AddAsync(It.IsAny<IEnumerable<Monster>>())).Returns(Task.CompletedTask);
        _repository.Setup(r => r.Save());

        var result = await _monsterController.ImportCsv(formFile);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        Assert.Equal("Wrong data mapping.", badRequestResult.Value);
    }
}
