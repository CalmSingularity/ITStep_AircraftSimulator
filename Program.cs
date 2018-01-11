using System;
using System.Collections.Generic;

namespace AircraftSimulator
{
	public delegate void ReportFlightDetails(bool overspeed = false); // для сообщения диспетчерам об изменении параметров полета и случаях превышения скорости

    static class Aircraft
    {
		public static bool ReachedMaxSpeed { get; private set; } = false; // индикатор достижения максимальной скорости
		public static bool Landed { get; private set; } = false;          // индикатор завершения полета

		/// <summary>
		/// Если полет завершен, меняет значение Landed на true
		/// </summary>
		private static void CheckIfLanded()
		{
			if (Speed == 0 && Altitude == 0 && ReachedMaxSpeed)
			{
				Landed = true;
			}
		}

		public static string MessageToPilot { get; set; } = "Чтобы начать полет, увеличьте скорость и высоту.";

		public static event ReportFlightDetails reportFlightDetails;

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
                    MessageToPilot = "Была зафиксирована максимальная скорость. Можно приступать к снижению и посадке.";
                }
                else
                {
                    speed = (value < 0) ? 0 : value;
                }

				// передача диспетчерам информации о полете:
				if (value > 1000)
                {
					reportFlightDetails(true);  // зафиксировано превышение максимальной скорости
                    MessageToPilot = "Максимальная скорость самолета 1000 км/ч! Можно приступать к снижению и посадке.";
                }
				else
				{
					reportFlightDetails(false); // превышения не было
				}

				CheckIfLanded();
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
                }
                else
				{
					MessageToPilot = "Невозможно изменить высоту при нулевой скорости.";
				}

				// передача диспетчерам информации о полете:
				reportFlightDetails();

				CheckIfLanded();
			}
        }

        public static List<Dispatcher> dispatchers = new List<Dispatcher>(2); // коллекция диспетчеров

        public static void AddDispatcher(string name)
        {
			Dispatcher dispatcher = new Dispatcher(name);
			dispatchers.Add(dispatcher);
			reportFlightDetails += dispatcher.CheckFlight;  // подписка диспетчера на изменение данных полета
        }

        public static void RemoveDispatcher(int index)
        {
            if (dispatchers.Count <= 2)
			{
				Aircraft.MessageToPilot = "Самолет должны контролировать минимум 2 диспетчера!";
				return;
			}
                
            if (index < 0 || index >= dispatchers.Count)
			{
				Aircraft.MessageToPilot = "Некорректный номер удаляемого диспетчера.";
				return;
			}
                
            PointsFromRemovedDispatchers += dispatchers[index].Points;  // сохранить штрафные очки от удаляемого диспетчера
			reportFlightDetails -= dispatchers[index].CheckFlight;  // отписка удаляемого диспетчера от изменения данных полета
            dispatchers.RemoveAt(index);  // удалить
        }

        public static int PointsFromRemovedDispatchers { get; private set; } = 0;
    }

    class Dispatcher
    {
        public string Name { get; }
		public int N { get; }  // корректировка погодных условий
		public int RecommendedAltitude { get; private set; }
        public string MessageToPilot { get; private set; }
        static Random rnd = new Random();

		private int points; // штрафные очки
		public int Points
		{
			get { return points; }
			private set
			{
				points = value;
				if (points >= 1000)
				{
					MessageToPilot = "Непригоден к полетам";
					// Исключение "Непригоден к полетам"
				}
			}
		}

		/// <summary>
		/// Рассчитывает рекомендуемую высоту и сохраняет в RecommendedAltitude
		/// </summary>
		private void CalculateRecommendedAltitude()
		{
			RecommendedAltitude = 7 * Aircraft.Speed - N;
			if (RecommendedAltitude < 0)
			{
				RecommendedAltitude = 0;
			}
		}

		public Dispatcher(string name)
        {
            Name = name;
            Points = 0;
            N = rnd.Next(-200, 201);  // задание корректировки от -200 до 200
			CalculateRecommendedAltitude();
        }

        public void CheckFlight(bool overspeed = false)
        {
			int difference = Math.Abs(Aircraft.Altitude - RecommendedAltitude);
			if (difference > 1000)
			{
				MessageToPilot = "Самолет разбился";
				// Исключение "самолет разбился"
			}
			else if (difference >= 600)
			{
				Points += 50;
				MessageToPilot = "Штраф 50 очков";
			}
			else if (difference >= 300)
			{
				Points += 25;
				MessageToPilot = "Штраф 25 очков";
			}
			else
			{
				MessageToPilot = "Полет нормальный";
			}

			if (overspeed)
			{
				Points += 100;
				MessageToPilot = "Снизить скорость!";
			}

			if (Aircraft.Speed == 0 && Aircraft.Altitude > 0)
            {
				MessageToPilot = "Самолет разбился";
				// Исключение "самолет разбился"
			}

			CalculateRecommendedAltitude();
		}
    }


    static class ConsoleUserInterface
    {
		/// <summary>
		/// Все возможные команды пользователя
		/// </summary>
		enum UserCommands
		{
			SpeedUp, SpeedUpFast, SpeedDown, SpeedDownFast,
			AltitudeUp, AltitudeUpFast, AltitudeDown, AltitudeDownFast,
			AddDispatcher, RemoveDispatcher, Exit
		};

		/// <summary>
		/// Ожидание и обработка команды пользователя
		/// </summary>
		static UserCommands GetCommand()
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

		/// <summary>
		/// Вывод всей информации о полете и рекомендаций диспетчеров
		/// </summary>
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
			Aircraft.MessageToPilot = "";
			Console.ResetColor();
            Console.WriteLine("------------------------------------------------------------------------------------------------\n");

			// Вывод списка диспетчеров, их рекомендаций, штрафных очков и суммы штрафных очков:
            int TotalPoints = 0, count = 1;
            foreach (Dispatcher dispatcher in Aircraft.dispatchers)
            {
                Console.WriteLine("Диспетчер {0, 2}: {1, -10} | Штрафные очки: {2, 4} | Рекомендуемая высота: {3, 5} м | {4}",
                    count, dispatcher.Name, dispatcher.Points, dispatcher.RecommendedAltitude, dispatcher.MessageToPilot);
				//dispatcher.MessageToPilot = "";
				TotalPoints += dispatcher.Points;
                ++count;
            }

            Console.WriteLine("\nОбщее количество штрафных очков: {0}\n",
                TotalPoints + Aircraft.PointsFromRemovedDispatchers);
        }

		/// <summary>
		/// Подготовка к полету
		/// </summary>
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
        }

		/// <summary>
		/// Управляет процессом полета, выполняя указания пользователя (пилота)
		/// </summary>
		public static bool Flight()
		{
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
						Console.Write("Введите номер удаляемого диспетчера: ");
						int index = Convert.ToInt32(Console.ReadLine());
						Aircraft.RemoveDispatcher(index - 1);
						break;
					case UserCommands.Exit:
						Console.WriteLine("Выход из программы. Полет не завершен.");
						return false;
				}

				ConsoleUserInterface.PrintFlightInfo();

			} while (!Aircraft.Landed);

			Console.WriteLine("Полет успешно завершен!");
			return true;
		} 

    }


    class Program
    {
        static void Main(string[] args)
        {
            ConsoleUserInterface.Start();

			ConsoleUserInterface.Flight();

			Console.WriteLine();

            // После завершения полета просуммировать все штрафные очки по всем диспетчерам

        }

    }
}