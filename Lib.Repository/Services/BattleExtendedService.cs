using Lib.Repository.Entities;
using Lib.Repository.Repository;

namespace Lib.Repository.Services.BattleExtendService
{
    public class BattleExtendedService
    {
        private readonly IBattleOfMonstersRepository _repository;

        public BattleExtendedService(IBattleOfMonstersRepository repository)
        {
            _repository = repository;
        }


        private Monster StartBattle(Monster monsterA, Monster monsterB)
        {
            int monsterAHp = monsterA.Hp;
            int monsterBhp = monsterB.Hp;

            Monster attacker = (monsterA.Speed > monsterB.Speed) || (monsterA.Speed == monsterB.Speed && monsterA.Attack > monsterB.Attack)
                ? monsterA : monsterB;

            Monster defender = attacker == monsterA ? monsterB : monsterA;

            while(monsterAHp > 0 && monsterBhp > 0)
            {
                int damage = attacker.Attack - defender.Defense;
                damage = damage > 0 ? damage : 1;

                if(defender == monsterA)
                {
                    monsterAHp -= damage;
                }
                else
                {
                    monsterBhp -= damage;
                }

                var change = attacker;
                attacker = defender;
                defender = change;
            }
            return monsterAHp <= 0 ? monsterB : monsterA;
        }

        public Battle CreateBattle(Monster monsterA, Monster monsterB)
        {
            Battle battle = new Battle();
            battle.MonsterA = monsterA.Id;
            battle.MonsterB = monsterB.Id;
            battle.MonsterARelation = monsterA;
            battle.MonsterBRelation = monsterB;

            Monster winner = StartBattle(monsterA, monsterB);
            battle.WinnerRelation = winner;
            battle.Winner = winner.Id;
            _repository.Battles.AddAsync(battle);
            _repository.Save();
            return battle;
        }

        public class BattleServiceFactory
        {
            public BattleExtendedService CreateBattleService(IBattleOfMonstersRepository _repository)
            {
                return new BattleExtendedService(_repository);
            }
        }
    }
}
