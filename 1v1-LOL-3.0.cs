using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FortniteTextEdition
{
    // Record type for efficient, immutable weapon data
    public record Weapon(string Name, int Damage);

    public class Player
    {
        public string Name { get; set; }
        public int Hp { get; private set; } = 100;
        public int Shield { get; private set; } = 0;
        public int Wood { get; set; } = 0;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public Weapon CurrentWeapon { get; set; } = new Weapon("Pickaxe", 10);
        public bool IsAlive => Hp > 0;

        public Player(string name)
        {
            Name = name;
        }

        public void AddShield(int amount)
        {
            Shield = Math.Min(100, Shield + amount);
        }

        public void TakeDamage(int amount)
        {
            if (Shield >= amount)
            {
                Shield -= amount;
            }
            else
            {
                amount -= Shield;
                Shield = 0;
                Hp = Math.Max(0, Hp - amount);
            }
        }
    }

    class Program
    {
        private const int MapSize = 20;
        private const int TotalPlayers = 100;
        private static readonly Random Rnd = new Random();

        static void Main(string[] args)
        {
            // Weapons Pool
            List<Weapon> lootPool = new()
            {
                new Weapon("Common Assault Rifle", 20),
                new Weapon("Rare SMG", 15),
                new Weapon("Epic Pump Shotgun", 45),
                new Weapon("Legendary SCAR", 35)
            };

            // Instantiate Human Player
            Player human = new("You (ProCoder)");

            // Instantiate 99 Bots via LINQ
            List<Player> lobby = Enumerable.Range(1, TotalPlayers - 1)
                .Select(i => new Player($"Bot_{i}")
                {
                    X = Rnd.Next(0, MapSize),
                    Y = Rnd.Next(0, MapSize),
                    CurrentWeapon = lootPool[Rnd.Next(lootPool.Count)],
                    Wood = Rnd.Next(0, 20)
                })
                .ToList();

            // Set random initial shield for bots
            lobby.ForEach(b => { if (Rnd.Next(2) == 0) b.AddShield(50); });

            int stormRadius = MapSize;
            int centerX = MapSize / 2;
            int centerY = MapSize / 2;

            Console.WriteLine("=== WELCOME TO FORTNITE (C# ADVANCED EDITION) ===");
            Console.WriteLine($"🚎 The Battle Bus is flying over a {MapSize}x{MapSize} grid island...\n");
            Thread.Sleep(1000);

            // Drop Phase
            Console.Write($"Enter landing coordinates X and Y (0 to {MapSize - 1}) separated by a space: ");
            string[] inputs = Console.ReadLine()?.Split(' ') ?? new string[] { "0", "0" };
            
            int.TryParse(inputs.ElementAtOrDefault(0), out int dropX);
            int.TryParse(inputs.ElementAtOrDefault(1), out int dropY);
            
            human.X = Math.Clamp(dropX, 0, MapSize - 1);
            human.Y = Math.Clamp(dropY, 0, MapSize - 1);

            // Loot Phase
            Console.WriteLine("\n📦 Landing... Opening a chest!");
            Thread.Sleep(1500);
            human.CurrentWeapon = lootPool[Rnd.Next(lootPool.Count)];
            human.AddShield(100);
            human.Wood = 30;

            Console.WriteLine($"Found: {human.CurrentWeapon.Name} ({human.CurrentWeapon.Damage} Dmg)!");
            Console.WriteLine("Found: 2x Shield Pots (+100 Shield)!");
            Console.WriteLine("Found: Materials (+30 Wood)!");
            Thread.Sleep(1500);

            // Main Game Loop
            while (human.IsAlive)
            {
                int aliveCount = lobby.Count(b => b.IsAlive) + 1;
                if (aliveCount == 1) break; // Victory Royale

                Console.WriteLine("\n================================================");
                Console.WriteLine($"👑 SURVIVORS: {aliveCount} / {TotalPlayers}");
                Console.WriteLine($"💖 HP: {human.Hp} | 🛡️ SHIELD: {human.Shield} | 🪵 WOOD: {human.Wood}");
                Console.WriteLine($"📍 LOCATION: ({human.X}, {human.Y}) | 🔫 WEAPON: {human.CurrentWeapon.Name}");
                Console.WriteLine($"⛈️ STORM EYE: Radius {stormRadius} around ({centerX}, {centerY})");
                Console.WriteLine("================================================");
                Thread.Sleep(1000);

                // 1. Storm Logic
                if (GetDistance(human.X, human.Y, centerX, centerY) > stormRadius)
                {
                    Console.WriteLine("⛈️ You are in the STORM! Taking 20 damage!");
                    human.TakeDamage(20);
                    if (!human.IsAlive) break;
                }

                // Passive background bot simulation
                int passiveElims = Rnd.Next(5, 20);
                foreach (var bot in lobby.Where(b => b.IsAlive))
                {
                    if (passiveElims <= 0) break;
                    if (Rnd.Next(3) == 0)
                    {
                        bot.TakeDamage(100); // Eliminate bot
                        passiveElims--;
                    }
                }

                // 2. Action Selector
                Console.WriteLine("\nWhat is your next play?");
                Console.WriteLine("1. Move toward the Safe Zone\n2. Farm Wood (+20 Wood)\n3. Hunt for nearby players");
                Console.Write("Choice: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        human.X += human.X < centerX ? 1 : (human.X > centerX ? -1 : 0);
                        human.Y += human.Y < centerY ? 1 : (human.Y > centerY ? -1 : 0);
                        Console.WriteLine($"🏃 You rotated toward safe center. Now at ({human.X}, {human.Y})");
                        break;
                    case "2":
                        human.Wood += 20;
                        Console.WriteLine($"🪓 You chopped down pixel trees. Total Wood: {human.Wood}");
                        break;
                    default:
                        Console.WriteLine("👀 Scanning the horizon for movement...");
                        break;
                }
                Thread.Sleep(1000);

                // 3. Combat Matchmaking
                Player enemy = lobby.FirstOrDefault(b => b.IsAlive && GetDistance(human.X, human.Y, b.X, b.Y) <= 3);

                if (enemy != null)
                {
                    Console.WriteLine($"\n❗ SPOTTED: {enemy.Name} is pushing your position!");
                    Thread.Sleep(1000);

                    // Build Mechanics
                    if (human.Wood >= 10)
                    {
                        human.Wood -= 10;
                        Console.WriteLine("🪵 You built a ramp! (Blocks enemy's first shot)");
                        Console.WriteLine($"🔫 You return fire with your {human.CurrentWeapon.Name}!");
                        enemy.TakeDamage(human.CurrentWeapon.Damage);
                        Console.WriteLine($"💥 Hit! {enemy.Name} HP is down to {enemy.Hp}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Out of materials! Open 50/50 shootout!");
                        Console.WriteLine($"🔫 You fire your {human.CurrentWeapon.Name}!");
                        enemy.TakeDamage(human.CurrentWeapon.Damage);

                        Console.WriteLine($"💥 {enemy.Name} lasers you back with {enemy.CurrentWeapon.Name}!");
                        human.TakeDamage(enemy.CurrentWeapon.Damage);
                    }

                    if (!enemy.IsAlive)
                    {
                        Console.WriteLine($"💀 ELIMINATED! You danced on {enemy.Name} and looted their shield cells!");
                        human.AddShield(50);
                    }
                    Thread.Sleep(1500);
                }
                else
                {
                    Console.WriteLine("💨 Quiet turn. No nearby firefights found.");
                    // Migrate living bots closer to center
                    foreach (var bot in lobby.Where(b => b.IsAlive))
                    {
                        bot.X += bot.X < centerX ? 1 : (bot.X > centerX ? -1 : 0);
                        bot.Y += bot.Y < centerY ? 1 : (bot.Y > centerY ? -1 : 0);
                    }
                }

                // 4. Shrink Circle
                if (stormRadius > 2)
                {
                    stormRadius -= 2;
                    Console.WriteLine("\n⚡ The Storm is shrinking! Safe zone is getting tighter! ⚡");
                }
                Thread.Sleep(1000);
            }

            // Results UI
            Console.WriteLine("\n================================================");
            if (human.IsAlive)
            {
                Console.WriteLine("🎉 🎉 🎉 #1 VICTORY ROYALE! 🎉 🎉 🎉");
                Console.WriteLine("You dominated the 100-player lobby in C#!");
            }
            else
            {
                Console.WriteLine("☠️ You were eliminated. Better luck next match! ☠️");
            }
            Console.WriteLine("================================================");
        }

        private static int GetDistance(int x1, int y1, int x2, int y2) => 
            Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }
}
