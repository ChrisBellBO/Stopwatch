using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LcdLabel
{
  /// <summary>
  /// Enumerated type defining the different types of display available
  /// </summary>
  public enum DotMatrix
  {
    /// <summary>
    /// 5 pixels by 7 pixels matrix
    /// </summary>
    mat5x7,
    /// <summary>
    /// 5 pixels by 8 pixels matrix
    /// </summary>
    mat5x8,
    /// <summary>
    /// 7 pixels by 9 pixels matrix
    /// </summary>
    mat7x9,
    /// <summary>
    /// 9 pixels by 12 pixels matrix
    /// </summary>
    mat9x12,
    /// <summary>
    /// Hitachi style
    /// </summary>
    Hitachi,
    /// <summary>
    /// Hitachi 2 style
    /// </summary>
    Hitachi2,
    /// <summary>
    /// DOS style display
    /// </summary>
    dos5x7
  }

  public sealed partial class LcdLabel : UserControl
  {
    public LcdLabel()
    {
      InitializeComponent();
      DefaultStyleKey = typeof(LcdLabel);
      Loaded += Label_Loaded;
    }

    private void Label_Loaded(object sender, RoutedEventArgs e)
    {
      DrawCharacters(true);
    }

    #region properties
    /// <summary>
    /// Gets or sets the number of text lines on the LCD.
    /// </summary>
    /// <value>The number of text lines on the LCD.</value>
    public int TextLines
    {
      get { return (int)GetValue(TextLinesProperty); }
      set
      {
        if (value < 1)
          throw new ArgumentException("Display needs at least one line");
        SetValue(TextLinesProperty, value);
      }
    }
    private static readonly DependencyProperty TextLinesProperty =
    DependencyProperty.Register(nameof(TextLines), typeof(int),
        typeof(LcdLabel), new PropertyMetadata(DependencyProperty.UnsetValue, OnValueChanged));

    /// <summary>
    /// Gets or sets the number of characters on a single line.
    /// </summary>
    /// <value>The number of characters on a single line.</value>
    public int NumberOfCharacters
    {
      get { return (int)GetValue(NumberOfCharactersProperty); }
      set
      {
        if (value < 1)
          throw new ArgumentException("Display needs at least one character");
        SetValue(NumberOfCharactersProperty, value);
      }
    }
    private static readonly DependencyProperty NumberOfCharactersProperty =
    DependencyProperty.Register(nameof(NumberOfCharacters), typeof(int),
        typeof(LcdLabel), new PropertyMetadata(DependencyProperty.UnsetValue, OnValueChanged));

    public int MatrixSpacing
    {
      get { return (int)GetValue(MatrixSpacingProperty); }
      set
      {
        if (value < 0)
          throw new ArgumentException("Matrix spacing must be a positive number or zero");
        SetValue(MatrixSpacingProperty, value);
      }
    }
    private static readonly DependencyProperty MatrixSpacingProperty =
    DependencyProperty.Register(nameof(MatrixSpacing), typeof(int),
        typeof(LcdLabel), new PropertyMetadata(0, OnValueChanged));

    public int LetterSpacing
    {
      get { return (int)GetValue(LetterSpacingProperty); }
      set
      {
        if (value < 0)
          throw new ArgumentException("Letter spacing must be a positive number or zero");
        SetValue(LetterSpacingProperty, value);
      }
    }
    private static readonly DependencyProperty LetterSpacingProperty =
    DependencyProperty.Register(nameof(LetterSpacing), typeof(int),
        typeof(LcdLabel), new PropertyMetadata(3, OnValueChanged));

    public Brush OffColour
    {
      get { return (Brush)GetValue(OffColourProperty); }
      set
      {
        SetValue(OffColourProperty, value);
      }
    }
    private static readonly DependencyProperty OffColourProperty =
    DependencyProperty.Register(nameof(OffColour), typeof(Brush),
        typeof(LcdLabel), new PropertyMetadata(DependencyProperty.UnsetValue, OnValueChanged));

    public Brush OnColour
    {
      get { return (Brush)GetValue(OnColourProperty); }
      set
      {
        SetValue(OnColourProperty, value);
      }
    }
    private static readonly DependencyProperty OnColourProperty =
    DependencyProperty.Register(nameof(OnColour), typeof(Brush),
        typeof(LcdLabel), new PropertyMetadata(DependencyProperty.UnsetValue, OnValueChanged));

    public string Text
    {
      get { return (string)GetValue(TextProperty); }
      set
      {
        SetValue(TextProperty, value);
      }
    }
    private static readonly DependencyProperty TextProperty =
    DependencyProperty.Register(nameof(Text), typeof(string),
        typeof(LcdLabel), new PropertyMetadata(DependencyProperty.UnsetValue, OnTextValueChanged));

    public DotMatrix DotMatrix
    {
      get { return (DotMatrix)GetValue(DotMatrixProperty); }
      set
      {
        SetValue(DotMatrixProperty, value);
      }
    }
    private static readonly DependencyProperty DotMatrixProperty =
    DependencyProperty.Register(nameof(DotMatrix), typeof(DotMatrix),
        typeof(LcdLabel), new PropertyMetadata(DotMatrix.mat5x7, OnValueChanged));

    #endregion

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var c = (LcdLabel)d;

      // Character drawing
      c.DrawCharacters(true);
    }

    private static void OnTextValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var c = (LcdLabel)d;

      // Character drawing
      c.DrawCharacters(false);
    }

    // temp
    private int first_c, last_c;
    private int pix_x;
    private int pix_y;

    private void DrawCharacters(bool rebuild)
    {
      if (LayoutRoot == null)
        return;
      if (Text == null)
        return;

      GetAsciiInterval();          // Calculate interval for ASCII values }
      CalcSize();                // Get Width and Height correct }

      var reallyRebuild = rebuild || LayoutRoot.RowDefinitions.Count == 0;

      if (reallyRebuild)
      {
        LayoutRoot.Children.Clear();
        // set grid rows
        LayoutRoot.RowDefinitions.Clear();
        for (var i = 0; i < TextLines; i++)
        {
          LayoutRoot.RowDefinitions.Add(new RowDefinition());
        }

        // set grid columns
        LayoutRoot.ColumnDefinitions.Clear();
        for (var i = 0; i < NumberOfCharacters; i++)
        {
          LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition());
        }
      }

      int cc;
      bool textend;

      cc = 1;
      textend = false;
      for (var row = 1; row <= TextLines; row++)
      {
        // Line counter             }
        for (var col = 1; col <= NumberOfCharacters; col++)
        {
          // Character counter        }
          if (!textend)              // Check for string end     }
            if (cc > Text.Length)
              textend = true;

          int charindex;
          if (textend)
            charindex = 0;
          else
            charindex = Convert.ToInt32(Text[cc - 1]);

          if (charindex < first_c)        // Limit charactes inside interval }
            charindex = first_c;

          if (charindex > last_c)
            charindex = last_c;

          DrawMatrix(col, row, charindex, reallyRebuild);
          cc++;
        }
      }
    }

    private void DrawMatrix(int col, int row, int charindex, bool rebuild)
    {
      // create the child grid
      Grid grid;
      if (rebuild)
      {
        grid = new Grid();
        LayoutRoot.Children.Add(grid);
        Grid.SetRow(grid, row - 1);
        Grid.SetColumn(grid, col - 1);
        grid.BorderThickness = new Thickness(LetterSpacing);
        grid.BorderBrush = Background;

      
        for (var i = 0; i < pix_y; i++)
        {
          grid.RowDefinitions.Add(new RowDefinition());
        }

        for (var i = 0; i < pix_x; i++)
        {
          grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
      }
      else
      {
        grid = (Grid)LayoutRoot.Children[(row-1) * TextLines + col-1];
      }

      charindex = charindex - first_c;
      
      for (var i = 0; i < pix_y; i++)
      {
        for (var j = 0; j < pix_x; j++)
        {
          Brush currentBrush;
          switch (DotMatrix)
          {
            case DotMatrix.mat5x7:
              if (Matrix.Char5x7[charindex, i, j] == 1)
                currentBrush = OnColour;
              else
                currentBrush = OffColour;
              break;
            case DotMatrix.mat5x8:
              if (Matrix.Char5x8[charindex, i, j] == 1)
                currentBrush = OnColour;
              else
                currentBrush = OffColour;
              break;
            case DotMatrix.Hitachi:
              if (Matrix.CharHitachi[charindex, i, j] == 1)
                currentBrush = OnColour;
              else
                currentBrush = OffColour;
              break;
            case DotMatrix.Hitachi2:
              // Use full height for character 194 - 223
              if (charindex <= 193)
              {  // Normal Hitachi
                if (i < 7)
                {
                  if (Matrix.CharHitachi[charindex, i, j] == 1)
                    currentBrush = OnColour;
                  else
                    currentBrush = OffColour;
                }
                else
                  currentBrush = OffColour;
              }
              else
              {
                // Extended height
                if (Matrix.CharHitachiExt[charindex, i, j] == 1)
                  currentBrush = OnColour;
                else
                  currentBrush = OffColour;
              }
              break;
            case DotMatrix.mat7x9:
              if (Matrix.Char7x9[charindex, i, j] == 1)
                currentBrush = OnColour;
              else
                currentBrush = OffColour;
              break;
            case DotMatrix.mat9x12:
              if (Matrix.Char9x12[charindex, i, j] == 1)
                currentBrush = OnColour;
              else
                currentBrush = OffColour;
              break;
            case DotMatrix.dos5x7:
              if (Matrix.CharDOS5x7[charindex, i, j] == 1)
                currentBrush = OnColour;
              else
                currentBrush = OffColour;
              break;
            default:
              throw new ArgumentException("Unrecognised DotMatrix - " + DotMatrix);
          }

          Rectangle rectangle1;
          if (rebuild)
          {
            rectangle1 = new Rectangle();
            grid.Children.Add(rectangle1);
            Grid.SetRow(rectangle1, i);
            Grid.SetColumn(rectangle1, j);
          }
          else
          {
            rectangle1 = (Rectangle) grid.Children[i * pix_x + j];
          }
          rectangle1.Fill = currentBrush;
          rectangle1.Stroke = Background;
          rectangle1.StrokeThickness = (double)MatrixSpacing / 2;
        }
      }
    }

    private void GetAsciiInterval()
    {
      switch (DotMatrix)
      {
        case DotMatrix.mat5x7:
        case DotMatrix.Hitachi:
          first_c = Matrix.HITACHI_FIRST;
          last_c = Matrix.HITACHI_LAST;
          break;
        case DotMatrix.Hitachi2:
          first_c = Matrix.HITACHI2_FIRST;
          last_c = Matrix.HITACHI2_LAST;
          break;
        case DotMatrix.mat5x8:
          first_c = Matrix.MAT5X8_FIRST;
          last_c = Matrix.MAT5X8_LAST;
          break;
        case DotMatrix.mat7x9:
          first_c = Matrix.MAT7X9_FIRST;
          last_c = Matrix.MAT7X9_LAST;
          break;
        case DotMatrix.mat9x12:
          first_c = Matrix.MAT9X12_FIRST;
          last_c = Matrix.MAT9X12_LAST;
          break;
        case DotMatrix.dos5x7:
          first_c = Matrix.DOS5X7_FIRST;
          last_c = Matrix.DOS5X7_LAST;
          break;
        default:
          throw new ArgumentException("Unrecognised DotMatrix - " + DotMatrix);
      }
    }

    private void CalcSize()
    {
      switch (DotMatrix)         //{ Calculate the space taken by the character matrix }
      {
        case DotMatrix.mat5x7:
          pix_x = Matrix.Char5x7.GetLength(2);
          pix_y = Matrix.Char5x7.GetLength(1);
          break;
        case DotMatrix.Hitachi:
          pix_x = Matrix.CharHitachi.GetLength(2);
          pix_y = Matrix.CharHitachi.GetLength(1);
          break;
        case DotMatrix.Hitachi2:
          pix_x = Matrix.CharHitachiExt.GetLength(2);
          pix_y = Matrix.CharHitachiExt.GetLength(1);
          break;
        case DotMatrix.mat5x8:
          pix_x = Matrix.Char5x8.GetLength(2);
          pix_y = Matrix.Char5x8.GetLength(1);
          break;
        case DotMatrix.mat7x9:
          pix_x = Matrix.Char7x9.GetLength(2);
          pix_y = Matrix.Char7x9.GetLength(1);
          break;
        case DotMatrix.mat9x12:
          pix_x = Matrix.Char9x12.GetLength(2);
          pix_y = Matrix.Char9x12.GetLength(1);
          break;
        case DotMatrix.dos5x7:
          pix_x = Matrix.CharDOS5x7.GetLength(2);
          pix_y = Matrix.CharDOS5x7.GetLength(1);
          break;
        default:
          throw new ArgumentException("Unrecognised DotMatrix - " + DotMatrix);
      }
    }
  }
}

