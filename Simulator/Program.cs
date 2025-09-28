using System;
using System.Linq;
using System.Collections.Generic;

public class PlayerStats
{
    public int level;
    public float attackDamage;
    public float abilityPower;
    public float attackSpeed;
    public float movementSpeed;
    public int diceStack;
    public int rewardBoxStack; // 누적된 '미개봉 보상 상자의 레벨' (0~10)

    public PlayerStats()
    {
        // 사용자가 요청한 시작 값 적용
        level = 10;
        attackDamage = 600;
        abilityPower = 600;
        attackSpeed = 1.1f;
        movementSpeed = 1.1f;
        diceStack = 0;
        rewardBoxStack = 0; // 시작 레벨은 0
    }
}

class Program
{
    private static readonly HashSet<int> bossLevels = new HashSet<int> { 11, 22, 33, 44, 55, 66, 77 };
    private static PlayerStats player = new PlayerStats();
    private static Random random = new Random();

    static void Main(string[] args)
    {
        Console.WriteLine("--- 주사위 시스템 시뮬레이터 (보스/레벨업 트리거) ---");
        Console.WriteLine("특정 레벨(11, 22 등)이 되면 보스를 처치한 것으로 간주하여 주사위가 굴러갑니다.");
        Console.WriteLine("엔터 키를 눌러 레벨업을 진행하세요. (종료: 'q' 입력 후 엔터)");
        Console.WriteLine("-----------------------------------------------------");

        while (true)
        {
            PrintPlayerStats();
            Console.Write("\n엔터를 눌러 레벨업 하세요... (현재 레벨: " + player.level + ")");
            var input = Console.ReadLine();
            if (input?.ToUpper() == "Q") break;
            Console.WriteLine("\n");

            LevelUp();
        }
    }

    static void LevelUp()
    {
        player.level++;
        Console.WriteLine($"플레이어 레벨이 {player.level}로 올랐습니다!");

        // 레벨업 시 주사위 굴리기 (1~6)
        RollDice();

        // 보스 처치 레벨에 도달하면 보스 주사위 굴리기 (2~7, 3번)
        if (bossLevels.Contains(player.level))
        {
            Console.WriteLine($"보스 몬스터를 처치했습니다! 주사위가 3번 더 굴러갑니다.");
            RollBossDice();
            RollBossDice();
            RollBossDice();
        }

        CheckRewardBox();
    }

    /// <summary>
    /// 일반 주사위를 굴리고 스택을 누적합니다. (눈금: 1~6)
    /// </summary>
    static void RollDice()
    {
        int diceRoll = random.Next(1, 7); // 1~6 주사위
        player.diceStack += diceRoll;
        Console.WriteLine($"[일반 주사위] {diceRoll}이(가) 나왔습니다. 주사위 스택: {player.diceStack}");
    }

    /// <summary>
    /// 보스 전용 주사위를 굴리고 스택을 누적합니다. (눈금: 2~7)
    /// </summary>
    static void RollBossDice()
    {
        int diceRoll = random.Next(2, 8); // 2~7 주사위
        player.diceStack += diceRoll;
        Console.WriteLine($"[보스 주사위] {diceRoll}이(가) 나왔습니다. 주사위 스택: {player.diceStack}");
    }

    /// <summary>
    /// 주사위 스택을 확인하고 보상 상자 레벨을 올립니다.
    /// </summary>
    static void CheckRewardBox()
    {
        bool levelIncreased = false; // 상자 레벨이 이번 레벨업에서 실제로 증가했는지 추적

        if (player.diceStack >= 30)
        {
            if (player.rewardBoxStack < 10)
            {
                int levelsPossible = player.diceStack / 30; // 획득 가능한 레벨 수
                int levelsToMax = 10 - player.rewardBoxStack; // 최대 레벨까지 남은 레벨 수
                
                int actualLevelsGained = Math.Min(levelsPossible, levelsToMax); // 실제로 획득한 레벨 수

                if (actualLevelsGained > 0)
                {
                    player.rewardBoxStack += actualLevelsGained;
                    
                    int diceConsumed = actualLevelsGained * 30;
                    player.diceStack -= diceConsumed; // 소모된 주사위 스택 제거

                    Console.WriteLine($"\n** 주사위 스택이 소모되며 보상 상자 레벨이 {actualLevelsGained}만큼 상승했습니다! 현재 레벨: {player.rewardBoxStack}. **");
                    levelIncreased = true; // 레벨이 증가했음을 표시
                }
            }
            else // player.rewardBoxStack == 10
            {
                // 최대 레벨에 도달한 경우, 주사위 스택은 다음 상자 개봉 후 재활용될 때까지 유지됩니다.
                Console.WriteLine("최대 보상 상자 레벨(10)에 도달하여 주사위 스택이 더 이상 레벨을 올리지 않습니다.");
            }
        }
        
        // 보상 상자 레벨이 1 이상이고, 이번 LevelUp() 호출로 레벨이 실제로 증가한 경우에만 개봉을 요청합니다.
        if (levelIncreased && player.rewardBoxStack > 0)
        {
            AskOpenBox();
        }
    }

    /// <summary>
    /// 상자 개봉 여부를 사용자에게 묻습니다. (한 번의 레벨 증가 시 한 번만 호출됨)
    /// </summary>
    static void AskOpenBox()
    {
        // AskOpenBox는 이제 CheckRewardBox에서 상자 레벨이 실제로 증가했을 때만 호출됩니다.
        // 따라서 무한 루프(while) 대신 한 번만 물어보고 응답을 처리합니다.
        Console.Write($"레벨 {player.rewardBoxStack} 보상 상자를 여시겠습니까? (Y/N): ");
        var input = Console.ReadLine()?.ToUpper();

        if (input == "Y")
        {
            OpenRewardBox(); 
        }
        else if (input == "N")
        {
            Console.WriteLine("상자를 열지 않고 레벨업을 진행합니다. 다음 주사위 스택이 쌓여 상자 레벨이 더 오를 수 있습니다.");
        }
        else
        {
            Console.WriteLine("잘못된 입력입니다. 'Y' 또는 'N'을 입력해주세요.");
        }
    }
    
    /// <summary>
    /// 보상 목록과 가중치를 반환합니다.
    /// </summary>
    static List<(Action<int, int> RewardAction, int Weight)> GetWeightedRewards(int rewardStackLevel)
    {
        var rewards = new List<(Action<int, int> RewardAction, int Weight)>
        {
            // Action<RewardStackLevel(1-10), RemainingDiceStack(0-29)>
            ((lvl, dice) => LevelUpCard(lvl, dice), 3),
            ((lvl, dice) => RandomItemBox(lvl, dice), 3),
            ((lvl, dice) => Console.WriteLine("3. 개발자의 편지 (히든 요소)"), 1), 
            ((lvl, dice) => GoldReward(lvl, dice), 3),
            ((lvl, dice) => StatBonus(lvl, dice), 3),
            ((lvl, dice) => DiceStackLoss(lvl, dice), 3),
            ((lvl, dice) => LuckCurseCard(lvl, dice), 3),
            ((lvl, dice) => Console.WriteLine("9. 특수 편지 (히든 스토리 요소)"), 1) 
        };

        // 요청하신 수정 사항: PermanentSpeedBonus는 3레벨 미만일 때 제외
        if (rewardStackLevel >= 3)
        {
            rewards.Add(((lvl, dice) => PermanentSpeedBonus(lvl, dice), 3));
        }
        // 참고: PermanentSpeedBonus가 제외될 경우 (1, 2레벨) 다른 보상이 선택될 확률이 상대적으로 올라갑니다.

        return rewards;
    }

    /// <summary>
    /// 현재 레벨에 해당하는 보상 상자를 개봉하고 보상을 적용합니다. (1회 실행)
    /// </summary>
    static void OpenRewardBox()
    {
        if (player.rewardBoxStack <= 0)
        {
            Console.WriteLine("개봉할 보상 상자가 없습니다.");
            return;
        }
        
        // 개봉할 상자의 레벨
        int rewardStackLevel = player.rewardBoxStack; 
        
        // 상자를 개봉했으므로 레벨 초기화
        player.rewardBoxStack = 0; 

        Console.WriteLine($"\n[상자 개봉] 레벨 {rewardStackLevel} 보상 상자를 엽니다.");
        Console.WriteLine("================= 획득한 보상 ==================");

        // 보상 목록을 상자 레벨에 맞춰 가져옵니다.
        var rewardsToChooseFrom = GetWeightedRewards(rewardStackLevel);
        List<Action<int, int>> selectedRewards = new List<Action<int, int>>();

        // 가중치 기반으로 무작위 3개 보상 선택
        for (int i = 0; i < 3 && rewardsToChooseFrom.Count > 0; i++)
        {
            int currentTotalWeight = rewardsToChooseFrom.Sum(r => r.Weight);
            if (currentTotalWeight <= 0) break;
            
            int selection = random.Next(currentTotalWeight);
            int cumulativeWeight = 0;
            
            for (int j = 0; j < rewardsToChooseFrom.Count; j++)
            {
                cumulativeWeight += rewardsToChooseFrom[j].Weight;
                if (selection < cumulativeWeight)
                {
                    // 선택된 보상 적용 및 리스트에서 제거 (고유 보상 보장)
                    selectedRewards.Add(rewardsToChooseFrom[j].RewardAction);
                    
                    // 리스트에서 제거
                    rewardsToChooseFrom.RemoveAt(j); 
                    break;
                }
            }
        }
        
        // 선택된 보상 적용
        foreach (var reward in selectedRewards)
        {
            // 상자 레벨 (rewardStackLevel)과 남은 주사위 스택(player.diceStack) 전달
            reward(rewardStackLevel, player.diceStack); 
        }

        Console.WriteLine("================================================");
        Console.WriteLine($"보상 상자 레벨이 0으로 초기화되었습니다. 다음 레벨업부터 주사위 스택을 다시 쌓아 상자 레벨을 올릴 수 있습니다.");
    }
    
    //----------------- 보상 효과 메서드 (Stack Level N=1~10 사용) -----------------

    /// <summary>
    /// 상자 레벨에 기반하여 Common, Rare, Legendary 등급을 가중치 기반으로 결정합니다.
    /// </summary>
    static string DetermineRarity(int rewardStackLevel)
    {
        // { (Common 가중치, Rare 가중치, Legendary 가중치) } -> 합계는 100
        // 상자 레벨 1~10 (인덱스 0~9)
        int[,] levelRarityWeights = new int[,] 
        {
            { 100, 0, 0 },    // Level 1: C 100%, R 0%, L 0%
            { 90, 10, 0 },    // Level 2: C 90%, R 10%, L 0%
            { 80, 20, 0 },    // Level 3: C 80%, R 20%, L 0%
            { 70, 20, 10 },   // Level 4: C 70%, R 20%, L 10%
            { 60, 30, 10 },   // Level 5: C 60%, R 30%, L 10%
            { 50, 35, 15 },   // Level 6: C 50%, R 35%, L 15%
            { 50, 30, 20 },   // Level 7: C 50%, R 30%, L 20%
            { 0, 80, 20 },    // Level 8: C 0%, R 80%, L 20%
            { 0, 70, 30 },    // Level 9: C 0%, R 70%, L 30%
            { 0, 60, 40 }     // Level 10: C 0%, R 60%, L 40%
        };

        if (rewardStackLevel < 1 || rewardStackLevel > 10)
        {
            return "Common"; // 기본값
        }

        int index = rewardStackLevel - 1;
        int commonWeight = levelRarityWeights[index, 0];
        int rareWeight = levelRarityWeights[index, 1];
        int legendaryWeight = levelRarityWeights[index, 2];
        
        int totalWeight = commonWeight + rareWeight + legendaryWeight;
        int selection = random.Next(totalWeight);

        if (selection < commonWeight)
        {
            return "Common";
        }
        else if (selection < commonWeight + rareWeight)
        {
            return "Rare";
        }
        else
        {
            return "Legendary";
        }
    }


    // LevelUpCard는 등급 판별을 위해 상자 레벨을 사용합니다.
    static void LevelUpCard(int rewardStackLevel, int remainingDiceStack)
    {
        string rarity = DetermineRarity(rewardStackLevel);
        
        // 기본 카드 장수 (1~3장)
        // 등급이 높아질수록 기본 카드 장수의 기대값이 높아짐
        int baseCards = rarity == "Common" ? random.Next(1, 2) : (rarity == "Rare" ? random.Next(1, 3) : random.Next(2, 4));
        
        // 상자 레벨에 비례하여 카드 장수 증가
        int totalCards = baseCards * rewardStackLevel;
        
        player.level += totalCards;
        Console.WriteLine($"1. 레벨업 카드 ({totalCards}장) 획득! (등급: {rarity}, 배율: {rewardStackLevel}) -> 현재 레벨: {player.level}");
    }

    static void RandomItemBox(int rewardStackLevel, int remainingDiceStack)
    {
        // 상자 레벨에 비례하여 아이템 수 증가 (1레벨 1개, 2레벨 2개...)
        Console.WriteLine($"2. 랜덤 아이템 박스 ({rewardStackLevel}개) 획득!");
    }
    
    static void GoldReward(int rewardStackLevel, int remainingDiceStack)
    {
        // 기본 골드
        int baseGold = random.Next(500, 1001);
        
        // 상자 레벨에 비례하여 배율 증가 (N레벨일 때 2^(N-1)배)
        long multiplier = (long)Math.Pow(2, rewardStackLevel - 1);
        
        // C# long을 사용하여 오버플로우 방지
        long totalGold = baseGold * multiplier;
        Console.WriteLine($"4. 골드 대량 획득! ({totalGold:N0} Gold, 배율: {multiplier}배)");
    }

    static void StatBonus(int rewardStackLevel, int remainingDiceStack)
    {
        // 보상 상자 레벨에 비례하여 보너스 증가 (N 레벨 = N * 5%)
        float bonus = 0.05f * rewardStackLevel; 
        player.attackDamage += player.attackDamage * bonus;
        player.abilityPower += player.abilityPower * bonus;
        Console.WriteLine($"5. STR/DEX/INT, AD/AP +{bonus * 100:F0}% (상자 레벨 {rewardStackLevel}만큼) -> AD: {player.attackDamage:F2}, AP: {player.abilityPower:F2}");
    }

    static void DiceStackLoss(int rewardStackLevel, int remainingDiceStack)
    {
        // 기존 로직 유지
        player.diceStack = Math.Max(0, remainingDiceStack - 3);
        Console.WriteLine($"6. 주사위 스택 3개 증발! -> 현재 스택: {player.diceStack}");
    }

    static void LuckCurseCard(int rewardStackLevel, int remainingDiceStack)
    {
        // 기존 로직 유지
        string[] buffs = { "공격력 10% 증가", "치명타 확률 5% 증가", "공격 속도 10% 증가", "방어력 5% 증가" };
        string[] curses = { "공격력 10% 감소", "이동 속도 10% 감소", "받는 피해 10% 증가", "쿨타임 10% 증가" };
        string buff = buffs[random.Next(buffs.Length)];
        string curse = curses[random.Next(curses.Length)];
        Console.WriteLine($"7. 행운/저주 카드 등장! -> [행운]: {buff}, [저주]: {curse}");
    }

    static void PermanentSpeedBonus(int rewardStackLevel, int remainingDiceStack)
    {
        // GetWeightedRewards()에서 이미 3레벨 미만은 제외했으므로, 여기서는 계산만 수행합니다.
        
        // 3레벨일 때 기본 0.1f, 1레벨 추가될 때마다 0.05f씩 증가
        float bonus = 0.1f + (rewardStackLevel - 3) * 0.05f;
        
        // 보너스 값이 음수가 되지 않도록 보호 (이 코드가 호출되는 시점에서는 rewardStackLevel >= 3)
        bonus = Math.Max(0f, bonus); 
        
        player.movementSpeed += bonus;
        player.attackSpeed += bonus;
        
        Console.WriteLine($"8. 이동속도/공격속도 +{(bonus * 100):F0}% (영구, 상자 레벨 {rewardStackLevel}) -> 이속: {player.movementSpeed:F2}, 공속: {player.attackSpeed:F2}");
    }
    
    static void PrintPlayerStats()
    {
        Console.WriteLine("\n--- 현재 스탯 ---");
        Console.WriteLine($"레벨: {player.level}");
        Console.WriteLine($"공격력(AD): {player.attackDamage:F2}");
        Console.WriteLine($"마법공격력(AP): {player.abilityPower:F2}");
        Console.WriteLine($"공격 속도: {player.attackSpeed:F2}");
        Console.WriteLine($"이동 속도: {player.movementSpeed:F2}");
        Console.WriteLine($"주사위 스택: {player.diceStack}");
        Console.WriteLine($"보상 상자 레벨: {player.rewardBoxStack}"); // 상자 레벨로 출력 변경
        Console.WriteLine("-------------------");
    }
}
