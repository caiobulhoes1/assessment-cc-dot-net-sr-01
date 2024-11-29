using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Lib.Repository.Services.BattleExtendService;
using Microsoft.AspNetCore.Mvc;
using static Lib.Repository.Services.BattleExtendService.BattleExtendedService;

namespace API.Controllers;


public class BattleExtendedController : BaseApiController
{
    private readonly IBattleOfMonstersRepository _repository;

    public BattleExtendedController(IBattleOfMonstersRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Add([FromBody] Battle battle)
    {
        if (battle.MonsterA == null || battle.MonsterB == null)
        {
            return BadRequest("Missing ID");
        }

        Monster monsterA = await _repository.Monsters.FindAsync(battle.MonsterA);
        Monster monsterB = await _repository.Monsters.FindAsync(battle.MonsterB);

        if (monsterA == null || monsterB == null)
        {
            return NotFound("One or both monsters not found.");
        }

        BattleServiceFactory factory = new BattleServiceFactory();
        BattleExtendedService battleService = factory.CreateBattleService(_repository);
        Battle battleSaved = battleService.CreateBattle(monsterA, monsterB);
        return Ok(battleSaved);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Remove(int id)
    {
        Battle battle = await _repository.Battles.FindAsync(id);
        if (battle == null)
        {
            return NotFound("Battle Not Found!");
        }

        await _repository.Battles.RemoveAsync(id);
        await _repository.Save();

        return NoContent();
    }
}
