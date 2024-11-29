using API.Controllers;
using API.Test.Fixtures;
using FluentAssertions;
using Lib.Repository;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Lib.Repository.Services.BattleExtendService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Test;

public class BattleExtendedControllerTests
{
    private readonly Mock<IBattleOfMonstersRepository> _repository;
    private readonly BattleOfMonstersContext _context;
    private readonly BattleOfMonstersRepository _repositorytest;

    public BattleExtendedControllerTests()
    {
        this._repository = new Mock<IBattleOfMonstersRepository>();
        var options = new DbContextOptionsBuilder<BattleOfMonstersContext>()
            .UseInMemoryDatabase(databaseName: "BattleOfMonstersTestDb").Options;

        _context = new BattleOfMonstersContext(options);
        _repositorytest = new BattleOfMonstersRepository(_context);
    }

    [Fact]
    public async Task Post_OnNoMonsterFound_When_StartBattle_With_NonexistentMonster()
    {
        var options = new DbContextOptionsBuilder<BattleOfMonstersContext>()
            .UseInMemoryDatabase(databaseName: "BattleOfMonstersTestPost").Options;

        using (var context = new BattleOfMonstersContext(options))
        {
            var monstersMock = MonsterFixture.GetMonstersMock().ToArray();
            await context.Monster.AddRangeAsync(monstersMock);
            await context.SaveChangesAsync();

            Assert.NotNull(monstersMock);
            Assert.NotEmpty(monstersMock);

            var repository = new BattleOfMonstersRepository(context);

            var battleRequest = new Battle()
            {
                MonsterA = monstersMock[0].Id,
                MonsterB = 98
            };

            var sut = new BattleExtendedController(repository);

            ActionResult result = await sut.Add(battleRequest);

            result.Should().BeOfType<NotFoundObjectResult>();
            var notfoundresult = result as NotFoundObjectResult;
            Assert.Equal("One or both monsters not found.", notfoundresult.Value);
        }
    }

    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterAWinning()
    {
        var monstersMock = MonsterFixture.GetMonstersMock().ToArray();

        var battleRequest = new Battle()
        {
            MonsterA = monstersMock[1].Id,
            MonsterB = monstersMock[2].Id,
        };

        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[1].Id)).ReturnsAsync(monstersMock[1]);
        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[2].Id)).ReturnsAsync(monstersMock[2]);

        var battleService = new BattleExtendedService(_repositorytest);
        Battle battle = battleService.CreateBattle(monstersMock[1], monstersMock[2]);

        _repository.Setup(x => x.Battles.AddAsync(It.IsAny<Battle>()))
            .Callback<Battle>(battle =>
            {
                battle.Winner = battle.WinnerRelation.Id;
                battle.WinnerRelation = battle.WinnerRelation;
            });

        var sut = new BattleExtendedController(_repository.Object);

        ActionResult result = await sut.Add(battleRequest);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var battleResult = okResult!.Value as Battle;

        Assert.NotNull(battleResult);
        Assert.Equal(monstersMock[1].Id, battleResult.Winner);
        Assert.Equal(monstersMock[1], battleResult.WinnerRelation);
    }



    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterBWinning()
    {
        var monstersMock = MonsterFixture.GetMonstersMock().ToArray();

        var battleRequest = new Battle()
        {
            MonsterA = monstersMock[2].Id,
            MonsterB = monstersMock[1].Id,
        };

        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[2].Id)).ReturnsAsync(monstersMock[2]);
        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[1].Id)).ReturnsAsync(monstersMock[1]);

        var battleService = new BattleExtendedService(_repositorytest);
        Battle battle = battleService.CreateBattle(monstersMock[2], monstersMock[1]);

        _repository.Setup(x => x.Battles.AddAsync(It.IsAny<Battle>()))
            .Callback<Battle>(battle =>
            {
                battle.Winner = battle.WinnerRelation.Id;
                battle.WinnerRelation = battle.WinnerRelation;
            });

        var sut = new BattleExtendedController(_repository.Object);

        ActionResult result = await sut.Add(battleRequest);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var battleResult = okResult!.Value as Battle;

        Assert.NotNull(battleResult);
        Assert.Equal(monstersMock[1].Id, battleResult.Winner);
        Assert.Equal(monstersMock[1], battleResult.WinnerRelation);
    }

    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterAWinning_When_TheirSpeedsSame_And_MonsterA_Has_Higher_Attack()
    {
        var monstersMock = MonsterFixture.GetMonstersMock().ToArray();

        var battleRequest = new Battle()
        {
            MonsterA = monstersMock[3].Id,
            MonsterB = monstersMock[4].Id,
        };

        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[3].Id)).ReturnsAsync(monstersMock[3]);
        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[4].Id)).ReturnsAsync(monstersMock[4]);

        var battleService = new BattleExtendedService(_repositorytest);
        Battle battle = battleService.CreateBattle(monstersMock[3], monstersMock[4]);

        _repository.Setup(x => x.Battles.AddAsync(It.IsAny<Battle>()))
            .Callback<Battle>(battle =>
            {
                battle.Winner = battle.WinnerRelation.Id;
                battle.WinnerRelation = battle.WinnerRelation;
            });

        var sut = new BattleExtendedController(_repository.Object);

        ActionResult result = await sut.Add(battleRequest);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var battleResult = okResult!.Value as Battle;

        Assert.NotNull(battleResult);
        Assert.Equal(monstersMock[3].Id, battleResult.Winner);
        Assert.Equal(monstersMock[3], battleResult.WinnerRelation);
    }

    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterBWinning_When_TheirSpeedsSame_And_MonsterB_Has_Higher_Attack()
    {
        var monstersMock = MonsterFixture.GetMonstersMock().ToArray();

        var battleRequest = new Battle()
        {
            MonsterA = monstersMock[4].Id,
            MonsterB = monstersMock[3].Id,
        };

        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[4].Id)).ReturnsAsync(monstersMock[4]);
        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[3].Id)).ReturnsAsync(monstersMock[3]);

        var battleService = new BattleExtendedService(_repositorytest);
        Battle battle = battleService.CreateBattle(monstersMock[4], monstersMock[3]);

        _repository.Setup(x => x.Battles.AddAsync(It.IsAny<Battle>()))
            .Callback<Battle>(battle =>
            {
                battle.Winner = battle.WinnerRelation.Id;
                battle.WinnerRelation = battle.WinnerRelation;
            });

        var sut = new BattleExtendedController(_repository.Object);

        ActionResult result = await sut.Add(battleRequest);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var battleResult = okResult!.Value as Battle;

        Assert.NotNull(battleResult);
        Assert.Equal(monstersMock[3].Id, battleResult.Winner);
        Assert.Equal(monstersMock[3], battleResult.WinnerRelation);
    }

    [Fact]
    public async Task Post_OnSuccess_Returns_With_MonsterAWinning_When_TheirDefensesSame_And_MonsterA_Has_Higher_Speed()
    {
        var monstersMock = MonsterFixture.GetMonstersMock().ToArray();

        var battleRequest = new Battle()
        {
            MonsterA = monstersMock[0].Id,
            MonsterB = monstersMock[2].Id,
        };

        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[0].Id)).ReturnsAsync(monstersMock[0]);
        _repository.Setup(x => x.Monsters.FindAsync(monstersMock[2].Id)).ReturnsAsync(monstersMock[2]);

        var battleService = new BattleExtendedService(_repositorytest);
        Battle battle = battleService.CreateBattle(monstersMock[0], monstersMock[2]);

        _repository.Setup(x => x.Battles.AddAsync(It.IsAny<Battle>()))
            .Callback<Battle>(battle =>
            {
                battle.Winner = battle.WinnerRelation.Id;
                battle.WinnerRelation = battle.WinnerRelation;
            });

        var sut = new BattleExtendedController(_repository.Object);

        ActionResult result = await sut.Add(battleRequest);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var battleResult = okResult!.Value as Battle;

        Assert.NotNull(battleResult);
        Assert.Equal(monstersMock[0].Id, battleResult.Winner);
        Assert.Equal(monstersMock[0], battleResult.WinnerRelation);
    }

    [Fact]
    public async Task Delete_OnSuccess_RemoveBattle()
    {
        var options = new DbContextOptionsBuilder<BattleOfMonstersContext>()
                    .UseInMemoryDatabase(databaseName: "BattleOfMonstersTestDelete").Options;

        using (var context = new BattleOfMonstersContext(options))
        {
            var monstersMock = BattlesFixture.GetBattlesMock().ToArray();
            await context.Battle.AddRangeAsync(monstersMock);
            await context.SaveChangesAsync();

            var battleDelete = monstersMock.First();
            int battleId = (int)battleDelete.Id;

            var repository = new BattleOfMonstersRepository(context);

            var sut = new BattleExtendedController(repository);

            ActionResult result = await sut.Remove(battleId);

            var removedBattle = await context.Battle.FindAsync(battleId);
            Assert.Null(removedBattle);
            result.Should().BeOfType<NoContentResult>();
        }
    }

    [Fact]
    public async Task Delete_OnNoBattleFound_Returns404()
    {
        int nonExistingBattle = 99;

        this._repository.Setup(x => x.Battles.FindAsync(nonExistingBattle));

        var sut = new BattleExtendedController(this._repository.Object);

        ActionResult result = await sut.Remove(nonExistingBattle);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
