using System;
using System.ComponentModel;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Elevator
{

    public partial class MainWindow : INotifyPropertyChanged
    {

        // =================================================================================================================================
        // =                                                         ПЕРЕМЕННЫЕ                                                            =
        // =================================================================================================================================
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        int iButton; // Номер кнопки, которую нажимает пользователь
        bool isOpen = true; // Открыты двери или нет?
        bool isMoving = false; // Движется лифт или нет?
        bool isBusy = false; // Занят ли лифт в данный момент?
        private int numFloor; // Номер этажа, на который нужно прибыть
        public int NumFloor
        {
            get { return numFloor; }
            set
            {
                if (numFloor != value)
                {
                    numFloor = value;
                    OnPropertyChanged();
                }
            }
        }
        private int currentFloor = 1; // Текущий этаж, на котором находится лифт в данный момент времени
        public int CurrentFloor
        {
            get { return currentFloor; }
            set
            {
                if (currentFloor != value)
                {
                    currentFloor = value;
                    OnPropertyChanged();
                }
            }
        }
        private double yOffset = 0; // К этим координатам будет привязан лифт
        public double YOffset
        {
            get { return yOffset; }
            set
            {
                if (yOffset != value)
                {
                    yOffset = value;
                    OnPropertyChanged();
                }
            }
        }       
        private double door1_xOffset = 525; // К этим координатам будет привязана левая дверь лифта
        public double Door1_xOffset
        {
            get { return door1_xOffset; }
            set
            {
                if (door1_xOffset != value)
                {
                    door1_xOffset = value;
                    OnPropertyChanged();
                }
            }
        }
        private double door2_xOffset = 574; // К этим координатам будет привязана правая дверь лифта
        public double Door2_xOffset
        {
            get { return door2_xOffset; }
            set
            {
                if (door2_xOffset != value)
                {
                    door2_xOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        // =================================================================================================================================
        // =                                                         ОСНОВНЫЕ МЕТОДЫ                                                       =
        // =================================================================================================================================
        public void OpenDoors() // Метод открытия дверей
        {
            // Проигрываем звук дверей
            SoundPlayer player = new SoundPlayer();
            player.Stream = Properties.Resources.doors;
            player.Play();
            // Получаем координаты дверей из xaml кода
            var elevatorDoor1 = (Rectangle)this.FindName("elevatorDoor1");
            var elevatorDoor2 = (Rectangle)this.FindName("elevatorDoor2");
            Door1_xOffset = Canvas.GetLeft(elevatorDoor1);
            Door2_xOffset = Canvas.GetLeft(elevatorDoor2);
            if (!isOpen & !isMoving) // Если двери закрыты и лифт не двигается, то открываем двери
            {
                isOpen = true;
                tMessage.Text += "\r\n Двери открываются...";
                Canvas.SetLeft(elevatorDoor1, Door1_xOffset -= 17); // Перемещаем левую дверь по оси X
                Canvas.SetLeft(elevatorDoor2, Door2_xOffset += 17); // Перемещаем правую дверь по оси X
            }
        }
        public void CloseDoors() // Метод закрытия дверей
        {
            // Проигрываем звук дверей
            SoundPlayer player = new SoundPlayer();
            player.Stream = Properties.Resources.doors;
            player.Play();
            // Получаем координаты дверей из xaml кода
            var elevatorDoor1 = (Rectangle)this.FindName("elevatorDoor1");
            var elevatorDoor2 = (Rectangle)this.FindName("elevatorDoor2");
            Door1_xOffset = Canvas.GetLeft(elevatorDoor1);
            Door2_xOffset = Canvas.GetLeft(elevatorDoor2);
            if (isOpen & !isMoving) // Если двери открыты и лифт не двигается, то закрываем двери
            {
                isOpen = false;
                tMessage.Text += "\r\n Двери закрываются...";
                Canvas.SetLeft(elevatorDoor1, Door1_xOffset += 17); // Перемещаем левую дверь по оси X
                Canvas.SetLeft(elevatorDoor2, Door2_xOffset -= 17); // Перемещаем правую дверь по оси X
            }
        }
        public async void MoveElevator() // Метод движения лифта
        {
            // Связываем координаты лифта и дверей с xaml кодом (т.к. двери перемещаются вместе с лифтом по оси Y)
            var elevatorBrush = (Rectangle)this.FindName("elevatorBrush");
            YOffset = Canvas.GetLeft(elevatorDoor1);
            YOffset = Canvas.GetLeft(elevatorDoor2);
            YOffset = Canvas.GetTop(elevatorBrush);
            if (CurrentFloor < NumFloor) // Если текущий этаж ниже нужного этажа, лифт едет вверх:
            {
                if (isOpen) // Если двери открыты, закрываем их
                {
                    await Task.Delay(1000); // Задержка 1 секунда
                    CloseDoors();
                }
                isMoving = true;
                await Task.Delay(1000);
                tMessage.Text += "\r\n Лифт едет на " + NumFloor + " этаж...";
                while (CurrentFloor != NumFloor) // Пока текущий этаж не равен нужному:
                {
                    // Проигрываем звук движения
                    SoundPlayer playerloop = new SoundPlayer();
                    playerloop.Stream = Properties.Resources.move_loop;
                    playerloop.Play();
                    // Создаем плавную анимацию перемещения лифта и его дверей на один этаж (по оси Y добавляем единицу)
                    // Всего 30 кадров с задержкой 50ms. Высота одного этажа равна 30
                    for (int i = 0; i < 30; i++)
                    {
                        YOffset -= 1;
                        Canvas.SetTop(elevatorDoor1, YOffset);
                        Canvas.SetTop(elevatorDoor2, YOffset);
                        Canvas.SetTop(elevatorBrush, YOffset);
                        await Task.Delay(50);
                    }
                    CurrentFloor++; // Теперь лифт находится на один этаж выше
                }
                // Проигрываем звук завершения движения
                SoundPlayer player = new SoundPlayer();
                player.Stream = Properties.Resources.stop;
                player.Play();
                tMessage.Text += "\r\n Лифт прибыл на " + CurrentFloor + " этаж";
                isMoving = false; // Больше не двигаемся
                if (!isOpen) // Если двери закрыты, открываем их с задержкой в 2 секунды
                {
                    await Task.Delay(2000);
                    OpenDoors();
                }
            }
            else if (CurrentFloor > NumFloor) // Тоже самое, только если текущий этаж выше нужного этажа, т.е. лифт едет вниз:
            {
                if (isOpen)
                {
                    await Task.Delay(1000);
                    CloseDoors();
                }
                isMoving = true;
                await Task.Delay(1000);
                tMessage.Text += "\r\n Лифт едет на " + NumFloor + " этаж...";
                while (CurrentFloor != NumFloor)
                {
                    SoundPlayer playerloop = new SoundPlayer();
                    playerloop.Stream = Properties.Resources.move_loop;
                    playerloop.Play();
                    for (int i = 0; i < 30; i++)
                    {
                        YOffset += 1;
                        Canvas.SetTop(elevatorDoor1, YOffset);
                        Canvas.SetTop(elevatorDoor2, YOffset);
                        Canvas.SetTop(elevatorBrush, YOffset);
                        await Task.Delay(50);
                    }
                    CurrentFloor--;
                }
                SoundPlayer player = new SoundPlayer();
                player.Stream = Properties.Resources.stop;
                player.Play();
                tMessage.Text += "\r\n Лифт прибыл на " + CurrentFloor + " этаж";
                isMoving = false;
                if (!isOpen)
                {
                    await Task.Delay(2000);
                    OpenDoors();
                }
            }
            else if (CurrentFloor == NumFloor) // Если нужный этаж равен текущему, то никуда не едем, просто сообщаем об этом пользователю
            {
                tMessage.Text += "\r\n Лифт уже находится на " + NumFloor + " этаже";
                await Task.Delay(1000);
            }
            isBusy = false; // Панель лифта освободилась от задач
        }
        public MainWindow() // Основной метод программы
        {
            DataContext = this;
            InitializeComponent();
        }

        // =================================================================================================================================
        // =                                                         МЕТОДЫ КНОПОК                                                         =
        // =================================================================================================================================
        private void ButFloor(object sender, RoutedEventArgs e) // Метод, вызываемый при нажатии кнопки этажа
        {
            Button button = (Button)sender;
            if (!isMoving & !isBusy) // Если лифт не едет, и если панель лифта не занята, то выполняем метод движения лифта
            {
                isBusy = true; // Панель лифта теперь занята
                Int32.TryParse(button.Content.ToString(), out iButton); // Достаем целое число из кнопки, которую нажал пользователь
                NumFloor = iButton; // Лифт должен прибыть на этаж, номер которого указал пользователь
                tMessage.Text = " Пользователь нажал кнопку " + iButton + "\r\n";
                MoveElevator();
            }
        }
        private void ButOpen(object sender, RoutedEventArgs e) // Метод кнопки открытия дверей
        {
            if (!isOpen & !isMoving)
            {
                OpenDoors();
            }
        }
        private void ButClose(object sender, RoutedEventArgs e) // Метод кнопки закрытия дверей
        {
            if (isOpen & !isMoving)
            {
                CloseDoors();
            }
        }
        private void ButHelp(object sender, RoutedEventArgs e) // Метод кнопки вызова диспетчера
        {
            tMessage.Text += "\r\n Диспетчер не отвечает";
        }
    }
}
