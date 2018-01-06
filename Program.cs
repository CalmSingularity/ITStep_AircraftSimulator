using System;
using System.Collections.Generic;

namespace AircraftSimulator
{
    static class Aircraft
    {
        private static int speed = 0;
        public static int Speed
        {
            get { return speed; }
            set
            {
                if (value >= 1000)
                {
                    speed = 1000;
                    ReachedMaxSpeed = true;
                    // Сообщить диспетчерам об изменении скорости
                    MessageToPilot = "Была зафиксирована максимальная скорость. Можно приступать к снижению и посадке.";
                }
                else
                {
                    speed = (value < 0) ? 0 : value;
                    MessageToPilot = "";
                }

                if (value > 1000)
                {
                    // Сообщить диспетчерам о попытке превышения скорости
                    MessageToPilot = "Максимальная скорость самолета 1000 км/ч. Можно приступать к снижению и посадке.";
                }
            }
        }

        private static int altitude = 0;
        public static int Altitude
        {
            get { return altitude; }
            set
            {
                if (speed > 0)
                {
                    altitude = (value < 0) ? 0 : value;
                    MessageToPilot = "";
                }
                else
                    MessageToPilot = "Невозможно изменить высоту при нулевой скорости.";
            }
        }

        public static List<Dispatcher> dispatchers = new List<Dispatcher>(2); // коллекция диспетчеров

        public static void AddDispatcher(string name)
        {
            dispatchers.Add(new Dispatcher(name));
        }

        public static void RemoveDispatcher(int index)
        {
            if (dispatchers.Count <= 2)
                return;
            if (index < 0 | index > dispatchers.Count)
                return;
            PointsFromRemovedDispatchers += dispatchers[index].Points;
            dispatchers.RemoveAt(index);
        }

        public static int PointsFromRemovedDispatchers { get; private set; } = 0;
        public static bool ReachedMaxSpeed { get; private set; }
        public static string MessageToPilot { get; set; } = "Чтобы начать полет, увеличьте скорость и высоту.";
    }

    class Dispatcher
    {
        public string Name { get; }
        public int N { get; }  // корректировка погодных условий
        public int Points { get; private set; }  // штрафные очки
        public string MessageToPilot { get; private set; }
        static Random rnd = new Random();
        public Dispatcher(string name)
        {
            Name = name;
            Points = 0;
            N = rnd.Next(-200, 201);  // задание корректировки от -200 до 200
        }

        public void CheckFlight()
        {
            if (Aircraft.Speed == 0 && Aircraft.Altitude > 0)
            {
                // самолет разбился
            }
        }

    }


    /// <summary>
    /// Все возможные команды пользователя
    /// </summary>
    enum UserCommands
    {
        SpeedUp, SpeedUpFast, SpeedDown, SpeedDownFast,
        AltitudeUp, AltitudeUpFast, AltitudeDown, AltitudeDownFast,
        AddDispatcher, RemoveDispatcher, Exit
    };

    static class ConsoleUserInterface
    {
        public static UserCommands GetCommand()
        {
            do
            {
                ConsoleKeyInfo command = Console.ReadKey(true);

                switch (command.Key)
                {
                    case ConsoleKey.Escape:  // выход из программы
                        return UserCommands.Exit;
                    case ConsoleKey.RightArrow:
                        if (command.Modifiers == 0)
                            return UserCommands.SpeedUp;
                        else
                            return UserCommands.SpeedUpFast; // нажат Shift, Alt или Ctrl
                    case ConsoleKey.LeftArrow:
                        if (command.Modifiers == 0)
                            return UserCommands.SpeedDown;
                        else
                            return UserCommands.SpeedDownFast; // нажат Shift, Alt или Ctrl
                    case ConsoleKey.UpArrow:
                        if (command.Modifiers == 0)
                            return UserCommands.AltitudeUp;
                        else
                            return UserCommands.AltitudeUpFast; // нажат Shift, Alt или Ctrl
                    case ConsoleKey.DownArrow:
                        if (command.Modifiers == 0)
                            return UserCommands.AltitudeDown;
                        else
                            return UserCommands.AltitudeDownFast; // нажат Shift, Alt или Ctrl
                    case ConsoleKey.A:
                        return UserCommands.AddDispatcher;
                    case ConsoleKey.R:
                        return UserCommands.RemoveDispatcher;
                }

            } while (true);  // пока пользователь не даст допустимую команду
        }

        public static void PrintFlightInfo()
        {
            Console.Clear();
            Console.WriteLine("-------------------------для управления используйте следующие клавиши:--------------------------");
            Console.WriteLine("  СНИЗИТЬ СКОРОСТЬ:   |  УВЕЛИЧИТЬ СКОРОСТЬ:   |    СНИЗИТЬ ВЫСОТУ:     |   УВЕЛИЧИТЬ ВЫСОТУ:   ");
            Console.WriteLine("     LEFT на  50 км/ч |      RIGHT на  50 км/ч |      DOWN на 250 м     |      UP на 250 м      ");
            Console.WriteLine("CTRL+LEFT на 150 км/ч | CTRL+RIGHT на 150 км/ч | CTRL+DOWN на 500 м     | CTRL+UP на 500 м      ");
            Console.WriteLine("------------------------------------------------------------------------------------------------");
            Console.WriteLine("  A - добавить диспетчера     R - удалить диспетчера     Esc - выход");
            Console.WriteLine("------------------------------------------------------------------------------------------------\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                СКОРОСТЬ ПОЛЕТА: {0, 4} км/ч           ВЫСОТА ПОЛЕТА: {1, 5} м\n",
                Aircraft.Speed, Aircraft.Altitude);
            Console.ResetColor();
            Console.WriteLine("------------------------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Aircraft.MessageToPilot);
            Console.ResetColor();
            Console.WriteLine("------------------------------------------------------------------------------------------------\n");

            int TotalPoints = 0, count = 1;
            foreach (Dispatcher dispatcher in Aircraft.dispatchers)
            {
                Console.WriteLine("Диспетчер {0, 2}: {1, -10} | Штрафные очки: {2, 4} | Рекомендуемая высота: {3, 5} м | {4}",
                    count, dispatcher.Name, dispatcher.Points, Aircraft.Altitude, dispatcher.MessageToPilot);
                TotalPoints += dispatcher.Points;
                ++count;
            }

            Console.WriteLine("\nОбщее количество штрафных очков: {0}\n",
                TotalPoints + Aircraft.PointsFromRemovedDispatchers);

            //Console.BackgroundColor = ConsoleColor.White;  // изменяет цвет фона                      
            //Console.ForegroundColor = ConsoleColor.DarkGreen;  // изменяет цвет текста
            //Console.ResetColor(); //устанавливает значение цвета текста в значение по умолчанию

        }

        public static void Start()
        {
            Console.Title = "Тренажер пилота самолета";
            Console.SetWindowSize(105, 25);
            Console.SetBufferSize(105, 25);
            Console.WriteLine("\nВас приветствует тренажер пилота самолета.\n");
            Console.Write("Введите имя первого диспетчера: ");
            string dispatcher1 = Console.ReadLine();
            Console.Write("Введите имя второго диспетчера: ");
            string dispatcher2 = Console.ReadLine();
            Aircraft.AddDispatcher(dispatcher1);
            Aircraft.AddDispatcher(dispatcher2);
            PrintFlightInfo();
            //Console.WriteLine("\n");
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            ConsoleUserInterface.Start();

            do
            {
                switch (ConsoleUserInterface.GetCommand())
                {
                    case UserCommands.SpeedUp:
                        Aircraft.Speed += 50;
                        break;
                    case UserCommands.SpeedUpFast:
                        Aircraft.Speed += 150;
                        break;
                    case UserCommands.SpeedDown:
                        Aircraft.Speed -= 50;
                        break;
                    case UserCommands.SpeedDownFast:
                        Aircraft.Speed -= 150;
                        break;
                    case UserCommands.AltitudeUp:
                        Aircraft.Altitude += 250;
                        break;
                    case UserCommands.AltitudeUpFast:
                        Aircraft.Altitude += 500;
                        break;
                    case UserCommands.AltitudeDown:
                        Aircraft.Altitude -= 250;
                        break;
                    case UserCommands.AltitudeDownFast:
                        Aircraft.Altitude -= 500;
                        break;
                    case UserCommands.AddDispatcher:
                        Console.Write("Введите имя нового диспетчера: ");
                        string name = Console.ReadLine();
                        Aircraft.AddDispatcher(name);
                        break;
                    case UserCommands.RemoveDispatcher:
                        if (Aircraft.dispatchers.Count <= 2)
                        {
                            Aircraft.MessageToPilot = "Самолет должны контролировать минимум 2 диспетчера!";
                            //Console.WriteLine("Самолет должны контролировать минимум 2 диспетчера!");
                            break;
                        }
                        Console.Write("Введите номер удаляемого диспетчера: ");
                        int index = Convert.ToInt32(Console.ReadLine());
                        Aircraft.RemoveDispatcher(index - 1);
                        break;
                    case UserCommands.Exit:
                        Console.WriteLine("\nВыход из программы. Полет не завершен.\n");
                        return;
                }

                ConsoleUserInterface.PrintFlightInfo();

            } while (Aircraft.Speed > 0 || Aircraft.Altitude > 0 || !Aircraft.ReachedMaxSpeed);

            // После завершения полета просуммировать все штрафные очки по всем диспетчерам

        }

    }
}