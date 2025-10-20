using MudBlazor;

namespace ButikProjesi.Istemci.Tema
{
    public class ButikTemasi
    {
        public static MudTheme Tema => new()
        {
            PaletteLight = new PaletteLight()
            {
                // Marka rengi: mor-mavi tonu
                Primary = "#667eea",
                PrimaryContrastText = Colors.Shades.White,
                PrimaryDarken = "#556bd3",
                PrimaryLighten = "#7a90ef",

                AppbarBackground = Colors.Shades.White,
                Background = "#F8F9FA", // Açık gri zemin
                
                TextPrimary = Colors.Shades.Black,
                
                // Diğer renkler için de kontrast ayarları
                Secondary = "#9E9E9E", // Gri renk
                SecondaryContrastText = Colors.Shades.Black,
                Dark = "#424242", // Koyu gri
                DarkContrastText = Colors.Shades.White,
                
                // Ek kontrast ayarları
                Error = "#F44336", // Kırmızı
                ErrorContrastText = Colors.Shades.White,
                Warning = "#FF9800", // Turuncu
                WarningContrastText = Colors.Shades.White,
                Info = "#2196F3", // Mavi
                InfoContrastText = Colors.Shades.White,
                Success = "#4CAF50", // Yeşil
                SuccessContrastText = Colors.Shades.White
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#8da2fb",
                PrimaryContrastText = Colors.Shades.Black,
                PrimaryDarken = "#7a90ef",
                PrimaryLighten = "#a4b4fd",
                
                AppbarBackground = "#303030", // Koyu gri
                Background = "#303030", // Koyu gri
                
                TextPrimary = Colors.Shades.White,
                
                // Diğer renkler için de kontrast ayarları
                Secondary = "#9E9E9E", // Gri renk
                SecondaryContrastText = Colors.Shades.Black,
                Dark = "#212121", // Çok koyu gri
                DarkContrastText = Colors.Shades.White,
                
                // Ek kontrast ayarları
                Error = "#F44336", // Kırmızı
                ErrorContrastText = Colors.Shades.White,
                Warning = "#FF9800", // Turuncu
                WarningContrastText = Colors.Shades.White,
                Info = "#2196F3", // Mavi
                InfoContrastText = Colors.Shades.White,
                Success = "#4CAF50", // Yeşil
                SuccessContrastText = Colors.Shades.White
            }
        };
    }
}
