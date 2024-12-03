using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Converters;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace AppCalculadoraIntereses
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class SimulacionAmortiguamiento
    {
        public int periodo { get; set; }
        public double interes { get; set; }
        public double amortizacion { get; set; }
        public double deudaRestante { get; set; }
    }
    public class Simulacion
    {
        public int Periodo { get; set; }
        public double CapitalInicial { get; set; }
        public double Interes { get; set; }
        public double CapitalAcumulado { get; set; }
    }



    public partial class MainWindow : Window
    {
        // Series de datos para los gráficos
        public ChartValues<double> InteresSimpleSeries { get; set; }
        public ChartValues<double> InteresCompuestoSeries { get; set; }
        public ChartValues<double> amortiguamientoFrancesSeries { get; set; }
        public ChartValues<double> interesFrancesSeries { get; set; }
        public List<string> LabelsAnios { get; set; }

        // Listas para las tablas de simulación
        public List<Simulacion> SimulacionInteresSimple { get; set; }
        public List<Simulacion> SimulacionInteresCompuesto { get; set; }
        public List<SimulacionAmortiguamiento> SimulacionAmortiguamiento { get; set; }
        public Func<double, string> FormatoLabelsEjeY { get; set; }

        public ChartValues<double> PagoPeriodicoSeries { get; set; }

        private ObservableCollection<AmortiguamientoResultado> _resultadosAmericano;

        public ChartValues<double> ValoresGrafico { get; set; }
        public ObservableCollection<string> LabelsPeriodos { get; set; }
        public ObservableCollection<ResultadoPeriodo> ResultadosTabla { get; set; }
        public Func<double, string> FormatoEjeY { get; set; }

        public ObservableCollection<AmortiguamientoResultado> ResultadosAmericano
        {
            get => _resultadosAmericano;
            set
            {
                _resultadosAmericano = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InteresSimpleSeries = new ChartValues<double>();
            InteresCompuestoSeries = new ChartValues<double>();
            amortiguamientoFrancesSeries = new ChartValues<double>();
            interesFrancesSeries = new ChartValues<double>();
            LabelsAnios = new List<string>();
            PagoPeriodicoSeries = new ChartValues<double>();
            ResultadosAmericano = new ObservableCollection<AmortiguamientoResultado>();
            DataContext = this; // Establecer el contexto de datos para enlazar con la UI
            ValoresGrafico = new ChartValues<double>();
            LabelsPeriodos = new ObservableCollection<string>();
            ResultadosTabla = new ObservableCollection<ResultadoPeriodo>();
            FormatoEjeY = value => $"${value:0.00}";
            tablaResultados.ItemsSource = ResultadosTabla;
            tablaResultados1.ItemsSource = ResultadosTabla;


            // Inicialización de las series para evitar null
            chartDepreciacion.Series = new LiveCharts.SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Title = "Valor en Libros",
                    Values = new LiveCharts.ChartValues<double>()
                },
                new LiveCharts.Wpf.LineSeries
                {
                    Title = "Depreciación Acumulada",
                    Values = new LiveCharts.ChartValues<double>()
                }
            };

            FormatoLabelsEjeY = value => $"S/. {value:N2}";

            DataContext = this;
        }

        private void ActualizarSimulacionAmortiguamientoAmericano(object sender, EventArgs e)
        {
            // Validar entradas
            if (!double.TryParse(txtCapitalAmortiguamientoAmericano.Text, out double capital) ||
                !double.TryParse(txtTasaAmortiguamientoAmericano.Text, out double tasa) ||
                !int.TryParse(txtMesesAmortiguamientoAmericano.Text, out int meses))
            {
                txtCuotaMensualAmericano.Text = "Ingrese valores válidos.";
                return;
            }

            tasa /= 100; // Convertir tasa a decimal
            ResultadosAmericano.Clear(); // Limpiar resultados anteriores
            var resultados = CalcularAmortiguamientoAmericano(capital, tasa, meses);

            // Actualizar resultados
            foreach (var resultado in resultados)
            {
                ResultadosAmericano.Add(resultado);
            }

            txtCuotaMensualAmericano.Text = $"Interés Mensual: {resultados[0].Interes:C}";
        }


        private List<AmortiguamientoResultado> CalcularAmortiguamientoAmericano(double capital, double tasa, int meses)
        {
            var resultados = new List<AmortiguamientoResultado>();

            for (int i = 1; i <= meses; i++)
            {
                var interes = capital * tasa / 12;
                resultados.Add(new AmortiguamientoResultado
                {
                    Periodo = i,
                    Interes = Math.Round(interes, 2),
                    Amortizacion = i == meses ? capital : 0,
                    DeudaRestante = i == meses ? 0 : capital
                });
            }

            return resultados;
        }

        private void BorrarDatosAmortiguamientoAmericano(object sender, RoutedEventArgs e)
        {
            txtCapitalAmortiguamientoAmericano.Text = "";
            txtTasaAmortiguamientoAmericano.Text = "";
            txtMesesAmortiguamientoAmericano.Text = "";
            ResultadosAmericano.Clear();
            txtCuotaMensualAmericano.Text = "Cuota mensual";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class AmortiguamientoResultado
        {
            public int Periodo { get; set; }
            public double Interes { get; set; }
            public double Amortizacion { get; set; }
            public double DeudaRestante { get; set; }
        }

        public class Depreciacion
        {
            public int Anio { get; set; }
            public decimal DepreciacionAnual { get; set; }
            public decimal DepreciacionAcumulada { get; set; }
            public decimal ValorEnLibros { get; set; }
        }

        private void CalcularAnualidad_Click(object sender, RoutedEventArgs e)
        {
            // Validar entradas
            if (!double.TryParse(txtMonto.Text, out double monto) ||
                !int.TryParse(txtPeriodos.Text, out int periodos) ||
                !double.TryParse(txtTasa.Text, out double tasa))
            {
                MessageBox.Show("Por favor ingrese valores válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            tasa /= 100; // Convertir tasa a decimal
            var resultados = CalcularAnualidadVencida(monto, tasa, periodos);

            // Mostrar resultados en la tabla
            tablaResultados.ItemsSource = resultados;
        }

        private List<AnualidadResultado> CalcularAnualidadVencida(double monto, double tasa, int periodos)
        {
            var resultados = new List<AnualidadResultado>();
            double valorAcumulado = 0;

            for (int i = 1; i <= periodos; i++)
            {
                valorAcumulado += monto * Math.Pow(1 + tasa, i);
                resultados.Add(new AnualidadResultado
                {
                    Periodo = i,
                    Pago = monto,
                    ValorAcumulado = Math.Round(valorAcumulado, 2)
                });
            }

            return resultados;
        }

        private void ActualizarDepreciacion(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtCostoInicial.Text, out decimal costoInicial) &&
                decimal.TryParse(txtValorRescate.Text, out decimal valorRescate) &&
                int.TryParse(txtVidaUtil.Text, out int vidaUtil) &&
                vidaUtil > 0)
            {
                decimal depreciacionAnual = (costoInicial - valorRescate) / vidaUtil;
                decimal depreciacionAcumulada = 0;
                txtResultadoDepreciacion.Text = $"Depreciación Anual: {depreciacionAnual:C}";

                var datosDepreciacion = new List<Depreciacion>();
                var valorEnLibros = costoInicial;

                // Agregar Periodo Cero
                datosDepreciacion.Add(new Depreciacion
                {
                    Anio = 0,
                    DepreciacionAnual = 0,
                    DepreciacionAcumulada = 0,
                    ValorEnLibros = costoInicial
                });

                // Agregar años de vida útil
                for (int i = 1; i <= vidaUtil; i++)
                {
                    depreciacionAcumulada += depreciacionAnual;
                    valorEnLibros -= depreciacionAnual;

                    datosDepreciacion.Add(new Depreciacion
                    {
                        Anio = i,
                        DepreciacionAnual = depreciacionAnual,
                        DepreciacionAcumulada = depreciacionAcumulada,
                        ValorEnLibros = Math.Max(valorEnLibros, valorRescate)
                    });
                }

                dataGridDepreciacion.ItemsSource = datosDepreciacion;

                // Actualizar la gráfica
                chartDepreciacion.Series[0].Values = new LiveCharts.ChartValues<decimal>(datosDepreciacion.Select(d => d.ValorEnLibros));
                chartDepreciacion.Series[1].Values = new LiveCharts.ChartValues<decimal>(datosDepreciacion.Select(d => d.DepreciacionAcumulada));
                chartDepreciacion.AxisX[0].Labels = datosDepreciacion.Select(d => d.Anio.ToString()).ToList();
            }
            else
            {
                txtResultadoDepreciacion.Text = "Por favor, verifica los datos ingresados.";
                dataGridDepreciacion.ItemsSource = null;
                chartDepreciacion.Series[0].Values.Clear();
                chartDepreciacion.Series[1].Values.Clear();
            }
        }


        private void BorrarDatosDepreciacion(object sender, RoutedEventArgs e)
        {
            txtCostoInicial.Clear();
            txtValorRescate.Clear();
            txtVidaUtil.Clear();
            txtResultadoDepreciacion.Text = "Resultado";
            dataGridDepreciacion.ItemsSource = null;

            // Limpiar gráfica
            chartDepreciacion.Series[0].Values.Clear();
            chartDepreciacion.Series[1].Values.Clear();
        }

        // Actualiza constantemente el resultado
        private void ActualizarSimulacionInteresSimple(object sender, TextChangedEventArgs e)
        {
            CalcularInteresSimple();
        }

        // Evento para calcular el interés simple
        private void CalcularInteresSimple()
        {
            try
            {
                // No hace nada si alguno de los tres campos esta vacio
                if (string.IsNullOrEmpty(txtCapitalSimple.Text) ||
                    string.IsNullOrEmpty(txtTasaSimple.Text) ||
                    string.IsNullOrEmpty(txtAniosSimple.Text))
                {
                    return;
                }

                double capital = Convert.ToDouble(txtCapitalSimple.Text);
                double tasa = Convert.ToDouble(txtTasaSimple.Text) / 100;
                double anios = Convert.ToDouble(txtAniosSimple.Text);

                InteresSimpleSeries.Clear();
                LabelsAnios.Clear();
                SimulacionInteresSimple = new List<Simulacion>();

                for (int i = 0; i <= anios; i++)
                {
                    double capitalInicial = capital;
                    double interes = capital * tasa * i;
                    double capitalAcumulado = capital + interes;

                    InteresSimpleSeries.Add(capitalAcumulado);
                    LabelsAnios.Add(i.ToString());

                    // Agregamos los datos a la tabla de simulación
                    SimulacionInteresSimple.Add(new Simulacion
                    {
                        Periodo = i,
                        CapitalInicial = capitalInicial,
                        Interes = interes,
                        CapitalAcumulado = capitalAcumulado
                    });
                }

                // Margenes arreglados
                chartInteresSimple.AxisY[0].MinValue = InteresSimpleSeries.Min();
                chartInteresSimple.AxisY[0].MaxValue = InteresSimpleSeries.Max();


                dataGridSimple.ItemsSource = SimulacionInteresSimple;
                double capitalFinal = Math.Round(InteresSimpleSeries[(int)anios], 2);
                txtResultadoSimple.Opacity = 1;
                txtResultadoSimple.Text = $"Capital Final: S/. {InteresSimpleSeries[(int)anios]}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en los datos ingresados. Verifique los campos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BorrarDatosInteresSimple(object sender, RoutedEventArgs e)
        {
            txtCapitalSimple.Text = "";
            txtTasaSimple.Text = "";
            txtAniosSimple.Text = "";

            if (InteresSimpleSeries != null)
            {
                InteresSimpleSeries.Clear();
            }

            if (SimulacionInteresSimple != null)
            {
                SimulacionInteresSimple.Clear();
                dataGridSimple.ItemsSource = null;
            }

        }

        // Actualiza constantemente el resultado
        private void ActualizarSimulacionInteresCompuesto(object sender, TextChangedEventArgs e)
        {
            CalcularInteresCompuesto();
        }

        // Evento para calcular el interés compuesto
        private void CalcularInteresCompuesto()
        {
            try
            {

                // No hace nada si alguno de los tres campos esta vacio
                if (string.IsNullOrEmpty(txtCapitalCompuesto.Text) ||
                    string.IsNullOrEmpty(txtTasaCompuesto.Text) ||
                    string.IsNullOrEmpty(txtAniosCompuesto.Text))
                {
                    return;
                }
                double capital = Convert.ToDouble(txtCapitalCompuesto.Text);
                double tasa = Convert.ToDouble(txtTasaCompuesto.Text) / 100;
                double anios = Convert.ToDouble(txtAniosCompuesto.Text);

                InteresCompuestoSeries.Clear();
                LabelsAnios.Clear();
                SimulacionInteresCompuesto = new List<Simulacion>();

                for (int i = 0; i <= anios; i++)
                {
                    double capitalInicial = capital;
                    double valorFuturo = capital * Math.Pow((1 + tasa), i);
                    double interes = valorFuturo - capital;

                    InteresCompuestoSeries.Add(valorFuturo);
                    LabelsAnios.Add(i.ToString());

                    // Agregamos los datos a la tabla de simulación
                    SimulacionInteresCompuesto.Add(new Simulacion
                    {
                        Periodo = i,
                        CapitalInicial = Math.Round(capitalInicial, 2),
                        Interes = Math.Round(interes, 2),
                        CapitalAcumulado = Math.Round(valorFuturo, 2)
                    });
                }

                double valorFuturoRedon = Math.Round(InteresCompuestoSeries[(int)anios], 2);
                double interesRedon = Math.Round(InteresCompuestoSeries[(int)anios] - capital, 2);

                // Margenes arreglados
                chartInteresCompuesto.AxisY[0].MinValue = InteresCompuestoSeries.Min();
                chartInteresCompuesto.AxisY[0].MaxValue = InteresCompuestoSeries.Max();

                dataGridCompuesto.ItemsSource = SimulacionInteresCompuesto;
                txtResultadoCompuesto.Opacity = 1;
                txtResultadoCompuesto.Text = $"Interés: S/. {interesRedon}\t\tValor futuro: S/. {valorFuturoRedon}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en los datos ingresados. Verifique los campos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BorrarDatosInteresCompuesto(object sender, RoutedEventArgs e)
        {
            txtCapitalCompuesto.Text = "";
            txtTasaCompuesto.Text = "";
            txtAniosCompuesto.Text = "";
            if (InteresCompuestoSeries != null)
            {
                InteresCompuestoSeries.Clear();
            }

            if (SimulacionInteresCompuesto != null)
            {
                SimulacionInteresCompuesto.Clear();
                dataGridCompuesto.ItemsSource = null;
            }

        }

        // Actualiza constantemente el resultado
        private void ActualizarSimulacionAmortiguamiento(object sender, TextChangedEventArgs e)
        {
            CalcularAmortiguamiento();
        }

        // Evento para calcular el amortiguamiento
        private void CalcularAmortiguamiento()
        {
            try
            {
                // No hace nada si alguno de los tres campos esta vacio
                if (string.IsNullOrEmpty(txtCapitalAmortiguamiento.Text) ||
                    string.IsNullOrEmpty(txtTasaAmortiguamiento.Text) ||
                    string.IsNullOrEmpty(txtMesesAmortiguamiento.Text))
                {
                    return;
                }

                double capital = Convert.ToDouble(txtCapitalAmortiguamiento.Text);
                double tasa = Convert.ToDouble(txtTasaAmortiguamiento.Text) / 100;
                double meses = Convert.ToDouble(txtMesesAmortiguamiento.Text);

                amortiguamientoFrancesSeries.Clear();
                interesFrancesSeries.Clear();
                LabelsAnios.Clear();
                SimulacionAmortiguamiento = new List<SimulacionAmortiguamiento>();

                double tasaMensual = Math.Round(tasa / 12, 4);
                double cuotaMensual = Math.Round((capital * tasaMensual * Math.Pow((1 + tasaMensual), meses)) / (Math.Pow((1 + tasaMensual), meses) - 1), 4);
                double deudaRestante = Math.Round(capital, 4);

                txtCuotaMensual.Text = "Cuota mensual fija: S/." + cuotaMensual.ToString();
                txtCuotaMensual.Opacity = 1;

                for (int i = 1; i <= meses; i++)
                {
                    double interes = deudaRestante * tasaMensual;
                    double amortiguamiento = cuotaMensual - interes;
                    deudaRestante -= amortiguamiento;

                    interesFrancesSeries.Add(interes);
                    amortiguamientoFrancesSeries.Add(amortiguamiento);
                    LabelsAnios.Add(i.ToString());

                    SimulacionAmortiguamiento.Add(new SimulacionAmortiguamiento
                    {
                        periodo = i,
                        amortizacion = Math.Round(amortiguamiento, 4),
                        interes = Math.Round(interes, 4),
                        deudaRestante = Math.Round(deudaRestante, 4),
                    });
                }

                dataGridAmortiguamiento.ItemsSource = SimulacionAmortiguamiento;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en los datos ingresados. Verifique los Campos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BorrarDatosAmortiguamiento(object sender, RoutedEventArgs e)
        {
            txtCapitalAmortiguamiento.Text = "";
            txtTasaAmortiguamiento.Text = "";
            txtMesesAmortiguamiento.Text = "";
            if (amortiguamientoFrancesSeries != null)
            {
                amortiguamientoFrancesSeries.Clear();
            }

            if (interesFrancesSeries != null)
            {
                interesFrancesSeries.Clear();
            }

            if (SimulacionAmortiguamiento != null)
            {
                SimulacionAmortiguamiento.Clear();
                dataGridAmortiguamiento.ItemsSource = null;
            }

        }

        // Lista para cálculos de VAN, TIR y Payback
        private List<double> flujosDeCaja = new List<double>();

        // Lista para la tabla de flujos de caja
        private List<FlujoCaja> flujosDeCajaTabla = new List<FlujoCaja>();


        private void AgregarPeriodoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string periodo = PeriodoTextBox.Text;
                string inversionStr = InversionTextBox.Text;
                string ingresosStr = IngresosTextBox.Text;

                if (string.IsNullOrWhiteSpace(periodo) || string.IsNullOrWhiteSpace(inversionStr) || string.IsNullOrWhiteSpace(ingresosStr))
                {
                    MessageBox.Show("Por favor, complete todos los campos.");
                    return;
                }

                double inversion = double.Parse(inversionStr);
                double ingresos = double.Parse(ingresosStr);
                double flujo = ingresos - inversion;

                flujosDeCaja.Add(flujo);
                flujosDeCajaTabla.Add(new FlujoCaja
                {
                    Periodo = periodo,
                    Inversion = inversion,
                    Ingresos = ingresos,
                    Flujo = flujo
                });

                FlujosDeCajaDataGrid.ItemsSource = null; // Forzar actualización
                FlujosDeCajaDataGrid.ItemsSource = flujosDeCajaTabla;

                MessageBox.Show($"Periodo {periodo}: Flujo añadido: {flujo}");
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar periodo: {ex.Message}");
            }
        }


        private void CalcularButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double tasaDescuento = double.Parse(TasaDescuentoTextBox.Text) / 100;

                if (!flujosDeCaja.Any())
                {
                    MessageBox.Show("Por favor, agregue flujos de efectivo antes de calcular.");
                    return;
                }

                double van = CalcularVAN(flujosDeCaja.ToArray(), tasaDescuento);
                double tir = CalcularTIR(flujosDeCaja.ToArray()) * 100;
                double payback = CalcularPayback(flujosDeCaja.ToArray());

                VANTextBox.Text = $"{van:F2} soles";
                TIRTextBox.Text = $"{tir:F2} %";
                PaybackTextBox.Text = payback >= 0 ? $"{payback:F2} años" : "No recuperado";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular: {ex.Message}");
            }
        }

        private double CalcularVAN(double[] flujos, double tasa)
        {
            double van = 0;
            for (int i = 0; i < flujos.Length; i++)
            {
                van += flujos[i] / Math.Pow(1 + tasa, i);
            }
            return van;
        }

        private double CalcularTIR(double[] flujos)
        {
            double tirInferior = 0.0;
            double tirSuperior = 1.0;
            double tir = 0.0;
            double tolerancia = 0.0001;

            while (tirSuperior - tirInferior > tolerancia)
            {
                tir = (tirInferior + tirSuperior) / 2;
                double van = CalcularVAN(flujos, tir);

                if (Math.Abs(van) < tolerancia)
                {
                    break;
                }
                else if (van > 0)
                {
                    tirInferior = tir;
                }
                else
                {
                    tirSuperior = tir;
                }
            }

            return tir;
        }

        private double CalcularPayback(double[] flujos)
        {
            double acumulado = 0;
            for (int i = 0; i < flujos.Length; i++)
            {
                acumulado += flujos[i];
                if (acumulado >= 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private void LimpiarCampos()
        {
            PeriodoTextBox.Clear();
            InversionTextBox.Clear();
            IngresosTextBox.Clear();
        }

        private void ActualizarCalculo(object sender, RoutedEventArgs e)
        {
            try
            {
                // Variables de entrada
                double A0 = double.Parse(txtValorBase.Text);
                double G = double.Parse(txtIncremento.Text);
                double i = double.Parse(txtTasaInteres.Text) / 100; // Convertir porcentaje
                int n = int.Parse(txtPeriodos1.Text);

                // Validación básica
                if (i <= 0 || n <= 0) throw new Exception("Tasa y periodos deben ser mayores que 0.");

                // Cálculo del valor presente total
                double P = CalcularValorPresente(A0, G, i, n);
                txtResultado.Text = $"Resultado: ${P:0.00}";

                // Actualizar gráfico y tabla
                ActualizarGraficoYTabla(A0, G, i, n);
            }
            catch
            {
                txtResultado.Text = "Por favor, verifica los datos ingresados.";
                ValoresGrafico.Clear();
                LabelsPeriodos.Clear();
                ResultadosTabla.Clear();
            }
        }
        private double CalcularValorPresente(double A0, double G, double i, int n)
        {
            double term1 = (A0 * (1 - Math.Pow(1 + i, -n))) / i;
            double term2 = (G / i) * (((1 - Math.Pow(1 + i, -n)) / i) - (n / Math.Pow(1 + i, n)));
            return term1 + term2;
        }

        private void ActualizarGraficoYTabla(double A0, double G, double i, int n)
        {
            ValoresGrafico.Clear();
            LabelsPeriodos.Clear();
            ResultadosTabla.Clear();

            for (int periodo = 1; periodo <= n; periodo++)
            {
                // Cálculo para cada periodo
                double valorAcumulado = A0 + (G * (periodo - 1));
                double valorPresente = CalcularValorPresente(A0, G, i, periodo);

                // Agregar datos al gráfico
                ValoresGrafico.Add(valorPresente);
                LabelsPeriodos.Add(periodo.ToString());

                // Agregar datos a la tabla
                ResultadosTabla.Add(new ResultadoPeriodo
                {
                    Periodo = periodo,
                    ValorAcumulado = valorAcumulado,
                    ValorPresente = valorPresente
                });
            }
        }

        private void BorrarDatos(object sender, RoutedEventArgs e)
        {
            txtValorBase.Clear();
            txtTasaInteres.Clear();
            txtIncremento.Clear();
            txtPeriodos1.Clear();
            txtResultado.Text = "Resultado: $0.00";
            ValoresGrafico.Clear();
            LabelsPeriodos.Clear();
            ResultadosTabla.Clear();
        }


    }

    public class Depreciacion
    {
        public int Anio { get; set; }
        public decimal DepreciacionAnual { get; set; }
        public decimal ValorEnLibros { get; set; }
    }

    public class AnualidadResultado
    {
        public int Periodo { get; set; }
        public double Pago { get; set; }
        public double ValorAcumulado { get; set; }
    }
    public class ResultadoPeriodo
    {
        public int Periodo { get; set; }
        public double ValorAcumulado { get; set; }
        public double ValorPresente { get; set; }
    }
    public class FlujoCaja
    {
        public string Periodo { get; set; }
        public double Inversion { get; set; }
        public double Ingresos { get; set; }
        public double Flujo { get; set; }
    }


}