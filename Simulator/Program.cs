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
    public int rewardBoxStack;

    public PlayerStats()
    {
        level = 1;
        attackDamage = 100;
        abilityPower = 100;
        attackSpeed = 1.0f;
        movementSpeed = 1.0f;
        diceStack = 0;
        rewardBoxStack = 0;
    }
}

class Program
{
    // 보스 처치 레벨
    private static readonly HashSet<int> bossLevels = new HashSet<int> { 11, 22, 33, 44, 55, 66, 77 };
    private static PlayerStats player = new PlayerStats();
    private static Random random = new Random();
    private static bool canOpenBox = false;

    static void Main(string[] args)
    {
        Console.WriteLine("--- 주사위 시스템 시뮬레이터 (보스/레벨업 트리거) ---");
        Console.WriteLine("특정 레벨(11, 22 등)이 되면 보스를 처치한 것으로 간주하여 주사위가 굴러갑니다.");
        Console.WriteLine("엔터 키를 눌러 레벨업을 진행하세요. (종료: 'q')");
        Console.WriteLine("-----------------------------------------------------");

        while (true)
        {
            PrintPlayerStats();
            Console.Write("\n엔터를 눌러 레벨업 하세요... (현재 레벨: " + player.level + ")");
            var input = Console.ReadKey();
            if (input.KeyChar.ToString().ToUpper() == "Q") break;
            Console.WriteLine("\n");

            LevelUp();
        }
    }

    /// <summary>
    /// 플레이어 레벨을 1 올리고 주사위 이벤트를 처리합니다.
    /// </summary>
    static void LevelUp()
    {
        player.level++;
        Console.WriteLine($"플레이어 레벨이 {player.level}로 올랐습니다!");

        // 레벨업 시 주사위 굴리기 (100% 확률)
        RollDice();

        // 보스 처치 레벨에 도달하면 주사위 다시 굴리기 (100% 확률)
        if (bossLevels.Contains(player.level))
        {
            Console.WriteLine($"보스 몬스터를 처치했습니다! 주사위가 한 번 더 굴러갑니다.");
            RollDice();
        }

        // 보상 상자가 쌓였는지 확인
        CheckRewardBox();
    }

    /// <summary>
    /// 주사위를 굴리고 스택을 누적합니다.
    /// </summary>
    static void RollDice()
    {
        int diceRoll = random.Next(1, 7); // 1~6 주사위
        player.diceStack += diceRoll;
        Console.WriteLine($"[주사위] {diceRoll}이(가) 나왔습니다. 주사위 스택: {player.diceStack}");
    }

    /// <summary>
    /// 주사위 스택을 확인하고, 보상 상자가 쌓였을 경우 플레이어에게 선택지를 줍니다.
    /// </summary>
    static void CheckRewardBox()
    {
        if (player.diceStack >= 30)
        {
            int newBoxes = player.diceStack / 30;
            player.rewardBoxStack += newBoxes;
            player.diceStack %= 30; // 30을 넘는 스택은 다음 스택으로 이월
            canOpenBox = true;
            Console.WriteLine($"\n** 보상 상자 스택이 {newBoxes}개 쌓였습니다! 현재 {player.rewardBoxStack}개. **");
            
            // 상자 열기 여부 질문
            AskOpenBox();
        }
        else
        {
            canOpenBox = false; // 상자가 쌓이지 않았다면 열 수 없음
        }
    }

    /// <summary>
    /// 보상 상자를 열지 말지 플레이어에게 질문하고 처리합니다.
    /// </summary>
    static void AskOpenBox()
    {
        while (canOpenBox)
        {
            Console.Write("보상 상자를 여시겠습니까? (Y/N): ");
            var input = Console.ReadLine()?.ToUpper();

            if (input == "Y")
            {
                OpenRewardBox();
                canOpenBox = false; // 한 번 열면 다음 스택이 쌓일 때까지 다시 열 수 없음
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
    /// 보상 상자를 열고 무작위 보상을 제공합니다.
    /// </summary>
    static void OpenRewardBox()
    {
        if (player.rewardBoxStack <= 0)
        {
            Console.WriteLine("보상 상자가 없습니다.");
            return;
        }

        player.rewardBoxStack--;
        Console.WriteLine($"\n[상자 개봉] 남은 보상 상자: {player.rewardBoxStack}개");
        Console.WriteLine("================= 획득한 보상 ==================");

        // 보상 목록에서 무작위 3개 선택
        var allRewards = new List<Action<int>>
        {
            (stack) => LevelUpCard(stack),
            (stack) => RandomItemBox(stack),
            (stack) => Console.WriteLine("3. 개발자의 편지 (히든 요소)"),
            (stack) => GoldReward(),
            (stack) => StatBonus(stack),
            (stack) => DiceStackLoss(),
            (stack) => LuckCurseCard(),
            (stack) => PermanentSpeedBonus(),
            (stack) => Console.WriteLine("9. 특수 편지 (히든 스토리 요소)")
        };

        // 보상 목록을 섞어서 무작위 3개 선택
        var selectedRewards = allRewards.OrderBy(x => random.Next()).Take(3).ToList();
        foreach (var reward in selectedRewards)
        {
            reward(player.diceStack); // 보상 효과 적용
        }

        Console.WriteLine("================================================");
    }
    
    //----------------- 보상 효과 메서드 -----------------
    static void LevelUpCard(int stack)
    {
        string rarity = stack <= 3 ? "Common" : (stack <= 6 ? "Rare" : "Legendary");
        int cards = rarity == "Common" ? random.Next(1, 2) : (rarity == "Rare" ? random.Next(1, 3) : random.Next(1, 4));
        player.level += cards;
        Console.WriteLine($"1. 레벨업 카드 ({cards}장) 획득! (등급: {rarity}) -> 현재 레벨: {player.level}");
    }

    static void RandomItemBox(int stack)
    {
        Console.WriteLine($"2. 랜덤 아이템 박스 ({stack}개) 획득!");
    }

    static void GoldReward()
    {
        int gold = random.Next(500, 1001);
        Console.WriteLine($"4. 골드 대량 획득! ({gold} Gold)");
    }

    static void StatBonus(int stack)
    {
        float bonus = 0.05f * stack;
        player.attackDamage += player.attackDamage * bonus;
        player.abilityPower += player.abilityPower * bonus;
        Console.WriteLine($"5. STR/DEX/INT, AD/AP +{bonus * 100}% (스택 {stack}만큼) -> AD: {player.attackDamage:F2}, AP: {player.abilityPower:F2}");
    }

    static void DiceStackLoss()
    {
        player.diceStack = Math.Max(0, player.diceStack - 3);
        Console.WriteLine($"6. 주사위 스택 3개 증발! -> 현재 스택: {player.diceStack}");
    }

    static void LuckCurseCard()
    {
        string[] buffs = { "공격력 10% 증가", "치명타 확률 5% 증가", "공격 속도 10% 증가", "방어력 5% 증가" };
        string[] curses = { "공격력 10% 감소", "이동 속도 10% 감소", "받는 피해 10% 증가", "쿨타임 10% 증가" };
        string buff = buffs[random.Next(buffs.Length)];
        string curse = curses[random.Next(curses.Length)];
        Console.WriteLine($"7. 행운/저주 카드 등장! -> [행운]: {buff}, [저주]: {curse}");
    }

    static void PermanentSpeedBonus()
    {
        player.movementSpeed += 0.1f;
        player.attackSpeed += 0.1f;
        Console.WriteLine($"8. 이동속도/공격속도 +10% (영구) -> 이속: {player.movementSpeed:F2}, 공속: {player.attackSpeed:F2}");
    }
    
    //----------------- 콘솔 출력 메서드 -----------------
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