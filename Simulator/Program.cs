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
    public int rewardBoxStack; // 누적된 미개봉 보상 상자 수

    public PlayerStats()
    {
        // 사용자가 요청한 시작 값 적용
        level = 10;
        attackDamage = 600;
        abilityPower = 600;
        attackSpeed = 1.1f;
        movementSpeed = 1.1f;
        diceStack = 0;
        rewardBoxStack = 0;
    }
}

class Program
{
    private static readonly HashSet<int> bossLevels = new HashSet<int> { 11, 22, 33, 44, 55, 66, 77 };
    private static PlayerStats player = new PlayerStats();
    private static Random random = new Random();
    private static bool canOpenBox = false;

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

        // 보스 처치 레벨에 도달하면 보스 주사위 굴리기 (2~7, 2번)
        if (bossLevels.Contains(player.level))
        {
            Console.WriteLine($"보스 몬스터를 처치했습니다! 주사위가 2번 더 굴러갑니다.");
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

    static void CheckRewardBox()
    {
        if (player.diceStack >= 30)
        {
            int newBoxes = player.diceStack / 30;
            
            // 최대 10스택 누적 가능 조건 처리 (CSV에 명시된 내용)
            if (player.rewardBoxStack + newBoxes > 10)
            {
                newBoxes = 10 - player.rewardBoxStack;
                if (newBoxes <= 0)
                {
                    Console.WriteLine("최대 보상 상자 스택(10개)에 도달하여 추가 적립되지 않았습니다.");
                    player.diceStack = 0; // 초과된 주사위 스택도 제거
                    return;
                }
            }
            
            player.rewardBoxStack += newBoxes;
            player.diceStack %= 30;
            canOpenBox = true;
            Console.WriteLine($"\n** 보상 상자 스택이 {newBoxes}개 쌓였습니다! 현재 {player.rewardBoxStack}개. **");
            
            AskOpenBox();
        }
        else
        {
            canOpenBox = false;
        }
    }

    static void AskOpenBox()
    {
        while (canOpenBox)
        {
            Console.Write("보상 상자를 여시겠습니까? (Y/N): ");
            var input = Console.ReadLine()?.ToUpper();

            if (input == "Y")
            {
                OpenRewardBox();
                canOpenBox = false;
            }
            else if (input == "N")
            {
                Console.WriteLine("상자를 열지 않고 진행합니다. 다음 스택이 쌓일 때까지 기다려야 합니다.");
                canOpenBox = false;
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다. 'Y' 또는 'N'을 입력해주세요.");
            }
        }
    }

    /// <summary>
    /// 보상 상자를 열고 무작위 보상을 제공합니다. (가중치 적용)
    /// </summary>
    static void OpenRewardBox()
    {
        if (player.rewardBoxStack <= 0)
        {
            Console.WriteLine("보상 상자가 없습니다.");
            return;
        }
        
        // 현재 보상 계산에 사용될 스택 레벨 (N=1 to 10)을 결정합니다.
        int rewardStackLevel = player.rewardBoxStack; 

        player.rewardBoxStack--;
        Console.WriteLine($"\n[상자 개봉] 남은 보상 상자: {player.rewardBoxStack}개");
        Console.WriteLine($"[보상 레벨] 스택 {rewardStackLevel}에 해당하는 보상을 획득합니다.");
        Console.WriteLine("================= 획득한 보상 ==================");

        // 보상 목록과 가중치 정의
        var weightedRewards = new List<(Action<int, int> RewardAction, int Weight)>
        {
            // Action<RewardStackLevel(1-10), RemainingDiceStack(0-29)>
            ((lvl, dice) => LevelUpCard(lvl, dice), 3),
            ((lvl, dice) => RandomItemBox(lvl, dice), 3),
            ((lvl, dice) => Console.WriteLine("3. 개발자의 편지 (히든 요소)"), 1), // 1/3 확률 적용 (가중치 1)
            ((lvl, dice) => GoldReward(lvl, dice), 3),
            ((lvl, dice) => StatBonus(lvl, dice), 3),
            ((lvl, dice) => DiceStackLoss(lvl, dice), 3),
            ((lvl, dice) => LuckCurseCard(lvl, dice), 3),
            ((lvl, dice) => PermanentSpeedBonus(lvl, dice), 3),
            ((lvl, dice) => Console.WriteLine("9. 특수 편지 (히든 스토리 요소)"), 1) // 1/3 확률 적용 (가중치 1)
        };

        // 가중치 기반으로 무작위 3개 보상 선택
        List<Action<int, int>> selectedRewards = new List<Action<int, int>>();
        int totalWeight = weightedRewards.Sum(r => r.Weight);

        for (int i = 0; i < 3; i++)
        {
            int selection = random.Next(totalWeight);
            int cumulativeWeight = 0;
            int selectedIndex = -1;

            for (int j = 0; j < weightedRewards.Count; j++)
            {
                cumulativeWeight += weightedRewards[j].Weight;
                if (selection < cumulativeWeight)
                {
                    selectedIndex = j;
                    break;
                }
            }

            if (selectedIndex != -1)
            {
                selectedRewards.Add(weightedRewards[selectedIndex].RewardAction);
                // 중복 보상을 피하기 위해 선택된 항목의 가중치를 0으로 설정
                // (리스트를 수정하는 대신 간단히 처리)
                weightedRewards[selectedIndex] = (weightedRewards[selectedIndex].RewardAction, 0); 
                totalWeight -= weightedRewards[selectedIndex].Weight;
            }
        }
        
        // 선택된 보상 적용 (남은 주사위 스택은 LevelUpCard의 등급 판별에만 사용)
        foreach (var reward in selectedRewards)
        {
            reward(rewardStackLevel, player.diceStack); 
        }

        Console.WriteLine("================================================");
    }
    
    //----------------- 보상 효과 메서드 (Stack Level N=1~10 사용) -----------------
    
    // LevelUpCard는 등급 판별을 위해 RemainingDiceStack을 사용합니다.
    static void LevelUpCard(int rewardStackLevel, int remainingDiceStack)
    {
        // 등급은 남은 주사위 스택(0~29)을 기준으로 합니다.
        string rarity = remainingDiceStack <= 3 ? "Common" : (remainingDiceStack <= 6 ? "Rare" : "Legendary");
        
        // 기본 카드 장수 (1~3장)
        int baseCards = rarity == "Common" ? random.Next(1, 2) : (rarity == "Rare" ? random.Next(1, 3) : random.Next(1, 4));
        
        // 스택 레벨에 비례하여 카드 장수 증가 (1스택 1배, 2스택 2배, 3스택 3배...)
        int totalCards = baseCards * rewardStackLevel;
        
        player.level += totalCards;
        Console.WriteLine($"1. 레벨업 카드 ({totalCards}장) 획득! (등급: {rarity}, 배율: {rewardStackLevel}) -> 현재 레벨: {player.level}");
    }

    static void RandomItemBox(int rewardStackLevel, int remainingDiceStack)
    {
        // 스택 레벨에 비례하여 아이템 수 증가 (1스택 1개, 2스택 2개...)
        Console.WriteLine($"2. 랜덤 아이템 박스 ({rewardStackLevel}개) 획득!");
    }
    
    static void GoldReward(int rewardStackLevel, int remainingDiceStack)
    {
        // 기본 골드
        int baseGold = random.Next(500, 1001);
        
        // 스택 레벨에 비례하여 배율 증가 (1스택 1배, 2스택 2배, 3스택 4배, 4스택 8배...)
        // N스택일 때 2^(N-1)배
        long multiplier = (long)Math.Pow(2, rewardStackLevel - 1);
        
        // C# long을 사용하여 오버플로우 방지
        long totalGold = baseGold * multiplier;
        Console.WriteLine($"4. 골드 대량 획득! ({totalGold:N0} Gold, 배율: {multiplier}배)");
    }

    static void StatBonus(int rewardStackLevel, int remainingDiceStack)
    {
        // 기존 로직 유지: 주사위 스택(0-29)에 비례
        float bonus = 0.05f * remainingDiceStack; 
        player.attackDamage += player.attackDamage * bonus;
        player.abilityPower += player.abilityPower * bonus;
        Console.WriteLine($"5. STR/DEX/INT, AD/AP +{bonus * 100}% (스택 {remainingDiceStack}만큼) -> AD: {player.attackDamage:F2}, AP: {player.abilityPower:F2}");
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
        float bonus = 0f;
        if (rewardStackLevel >= 3)
        {
            // 3스택일 때 기본 0.1f, 1스택 추가될 때마다 0.05f씩 증가
            bonus = 0.1f + (rewardStackLevel - 3) * 0.05f;
            
            // 보너스 값이 음수가 되지 않도록 보호 (3스택 미만 시 0)
            bonus = Math.Max(0f, bonus); 
        }
        
        player.movementSpeed += bonus;
        player.attackSpeed += bonus;
        
        Console.WriteLine($"8. 이동속도/공격속도 +{(bonus * 100):F0}% (영구, 스택 레벨 {rewardStackLevel}) -> 이속: {player.movementSpeed:F2}, 공속: {player.attackSpeed:F2}");
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
        Console.WriteLine($"보상 상자: {player.rewardBoxStack}개");
        Console.WriteLine("-------------------");
    }
}
