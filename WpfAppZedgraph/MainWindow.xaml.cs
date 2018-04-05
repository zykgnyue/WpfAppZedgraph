using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZedGraph;
namespace WpfAppZedgraph
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        int tickStart = 0;
        Timer timer1;
        public MainWindow()
        {
            InitializeComponent();
            TextBlock txtBlk = new TextBlock();
            txtBlk.Text = "Dynamic Add";
            gridMain.Children.Add(txtBlk);
            
        }

        private void SetGraph()
        {
            GraphPane myPane = zedgraph.GraphPane;
            //zedgraph.GraphPane.Title.Text = "This is a dynamic chart";

            /// 设置标题
            myPane.Title.Text = "Test of Dynamic Data Update with ZedGraph " + "(After  25 seconds the graph scrolls)";
            /// 设置X轴说明文字
            myPane.XAxis.Title.Text = "Time, Seconds";
            /// 设置Y轴文字
            myPane.YAxis.Title.Text = "Sample Potential, Volts1";
            myPane.Y2Axis.Title.Text = "Sample Potential, Volts2";
            /// Save 1200 points. At 50 ms sample rate, this is one minute 
            /// The RollingPointPairList is an efficient storage class that always 
            /// keeps a rolling set of point data without needing to shift any data values 
            /// 设置1200个点，假设每50毫秒更新一次，刚好检测1分钟
            /// 一旦构造后将不能更改这个值
            //RollingPointPairList 
            //IPointList 
            RollingPointPairList list1 = new RollingPointPairList(1200);
            RollingPointPairList list2 = new RollingPointPairList(1200);
            /// Initially, a curve is added with no data points (list is empty) 
            /// Color is blue,  and there will be no symbols 
            /// 开始，增加的线是没有数据点的(也就是list为空)   
            ///增加一条名称 :Voltage ，颜色 Color.Bule ，无符号，无数据的空线条

            LineItem curve1 = myPane.AddCurve("Voltage1", list1,System.Drawing.Color.Blue, SymbolType.None/*.Diamond*/ );
            LineItem curve2 = myPane.AddCurve("Voltage2", list2, System.Drawing.Color.Red, SymbolType.None);

            curve2.IsY2Axis = true;
            myPane.Y2Axis.IsVisible = true;
            myPane.Y2Axis.Scale.Min = -5.0;
            myPane.Y2Axis.Scale.Max = 5.0;
            // Align the Y2 axis labels so they are flush to the axis
            myPane.Y2Axis.Scale.Align = AlignP.Inside;
            //curve2.YAxisIndex = 1;

            myPane.Y2Axis.Scale.MaxAuto=false;
            myPane.Y2Axis.Scale.MinAuto =false;

            /// Just manually control the X axis range so it scrolls continuously 
            /// instead of discrete step-sized jumps 
            /// X 轴最小值 0  

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.MaxGrace = 0.01;
            myPane.XAxis.Scale.MinGrace = 0.01;
            /// X轴最大30 

            myPane.XAxis.Scale.Max = 30;

            /// X轴小步长1,也就是小间隔


            myPane.XAxis.Scale.MinorStep = 1;

            /// X轴大步长为5，也就是显示文字的大间隔


            myPane.XAxis.Scale.MajorStep = 5;

            /// Scale the axes 
            /// 改变轴的刻度


            zedgraph.AxisChange();
            
            /// Save the beginning time for reference 
            ///保存开始时间
            tickStart = Environment.TickCount;

            timer1 = new Timer(50);
            timer1.Elapsed += (sender, e) => HandleTimer(sender, e);
            //timer1.Elapsed += HandleTimer(this, new RoutedEventArgs());

            //timer1.Start();
            /// Sample at 50ms intervals 
            ///设置 timer 控件的间隔为 50 毫秒
            //timer1.Interval = 50;

            ///timer 可用
            timer1.Enabled = true;

            ///开始


            timer1.Start();



        }

        //private ElapsedEventHandler HandleTimer(object sender,  RoutedEventArgs e)
        //private  Task HandleTimer()
            public void HandleTimer(object source, System.Timers.ElapsedEventArgs e)
            {
            
                // Make sure that the curvelist has at least one curve 
                //确保CurveList不为空
                if (zedgraph.GraphPane.CurveList.Count <= 0) return;

                // Get the  first CurveItem in the graph 

                //取Graph第一个曲线，也就是第一步:在 GraphPane.CurveList 集合中查找 CurveItem 
                for(int idxList=0;idxList< zedgraph.GraphPane.CurveList.Count;idxList++)
                {
                    LineItem curve = zedgraph.GraphPane.CurveList[idxList] as LineItem;
                    if (curve == null) return;

                    // Get the PointPairList 
                    //第二步:在CurveItem中访问PointPairList(或者其它的IPointList)，根据自己的需要增加新数据或修改已存在的数据
                
                    IPointListEdit list = curve.Points as IPointListEdit;

                    // If this is null, it means the reference at curve.Points does not  
                    // support IPointListEdit, so we won't be able to modify it 

                    if (list == null) return;


                    // Time is measured in seconds 
                    double time = (Environment.TickCount - tickStart) / 1000.0;
                    // 3 seconds per cycle 

                    list.Add(time, (1+ idxList)*Math.Sin((2.0) * Math.PI * time / 3.0));

                    // Keep the X scale at a rolling 30 second interval, with one 

                    // major step between the max X value and the end of the axis 
                    Scale xScale = zedgraph.GraphPane.XAxis.Scale;
                    if (time > xScale.Max - xScale.MajorStep)
                    {
                        xScale.Max = time + xScale.MajorStep;
                        xScale.Min = xScale.Max - 30.0;
                    }

                }
                // Make sure the Y axis is rescaled to accommodate actual data 
                //第三步:调用ZedGraphControl.AxisChange()方法更新X和Y轴的范围


                zedgraph.AxisChange(); // Force a redraw  
                                       //第四步:调用Form.Invalidate()方法更新图表


                zedgraph.Invalidate();
                return;

        }

        private void buttonAddPoint_Click(object sender, RoutedEventArgs e)
        {
            MainWindow xx = new MainWindow();
            xx.SetGraph();
            xx.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timer1!= null){
                timer1.Enabled = false;
            }
        }
    }
}
