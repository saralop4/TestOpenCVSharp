using OpenCvSharp;
using System.Drawing;

namespace TestOpenCVSharp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string nameVideo;
            string pathImage;
            string pathVideo = GetValidVideoPath();
            while (!Directory.Exists(pathVideo))
            {
                Console.WriteLine("Ingresó una ruta de video inválida. Intente de nuevo!");
                Console.WriteLine("***********Validando Ruta de Video************");
                Console.WriteLine("");
                pathVideo = GetValidVideoPath();
            }

            // Obtiene todos los archivos en el directorio
            string[] files = Directory.GetFiles(pathVideo);

            Console.WriteLine("Archivos en " + pathVideo + ":");
            foreach (string file in files)
            {
                Console.WriteLine("NOMBRE DEl VIDEO: " + "{ " + Path.GetFileName(file) + " }"); // Muestra solo el nombre del archivo
            }



            nameVideo = GetValidNameVideo();
            string fullPathVideo = Path.Combine(pathVideo, nameVideo);
            Console.WriteLine("valor de full 1: " + fullPathVideo);
            while (!File.Exists(fullPathVideo))
            {

                Console.WriteLine("Nombre del video invalido o el archivo no existe.");
                Console.WriteLine("***********Validando Nombre del Video************");
                Console.WriteLine("");
                nameVideo = GetValidNameVideo();
                fullPathVideo = Path.Combine(pathVideo, nameVideo);
                Console.WriteLine("valor de full 2: " + fullPathVideo);

            }


            pathImage = GetValidNameFileImage();

            while (true)
            {
                Console.WriteLine("Path para los frames: " + pathImage);

                if (string.IsNullOrWhiteSpace(pathImage))
                {
                    Console.WriteLine("Ingrese una ruta de imagen válida. Intente de nuevo");
                }
                else
                {
                    // Verifica si el directorio existe
                    if (!Directory.Exists(pathImage))
                    {
                        try
                        {
                            // Si no existe, intenta crear el directorio
                            Directory.CreateDirectory(pathImage);
                            Console.WriteLine("El directorio no existía y ha sido creado.");
                            Console.Write("Path de los frames: "+ pathImage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"No se pudo crear el directorio. Error: {ex.Message}. Intente de nuevo:");
                        }
                    }
                    break; // Salir del bucle si la ruta es válida y el directorio existe o fue creado
                }
                pathImage = GetValidNameFileImage(); // Vuelve a solicitar la ruta
            }


            string filenameImg = GetValidFileName();
            if (string.IsNullOrWhiteSpace(filenameImg))
            {
                Console.WriteLine("No ingresó un nombre de archivo válido.");
                return;
            }

            string format = GetValidFormat();
            if (string.IsNullOrWhiteSpace(format))
            {
                Console.WriteLine("No ingresó un formato válido.");
                return;
            }

            ProcessVideo(fullPathVideo, pathImage, filenameImg, format);
        }

        private static void ProcessVideo(string fullPathVideo, string pathImage, string filenameImg, string format)
        {
            using var videoCapture = new VideoCapture(fullPathVideo);
            if (!videoCapture.IsOpened())
            {
                Console.WriteLine("Error: No se pudo abrir el video.");
                return;
            }

            int imgIndex = 0;

            // Crear la ventana antes del bucle para evitar recrearla en cada iteración
            Cv2.NamedWindow("Frame", WindowFlags.AutoSize);

            while (videoCapture.IsOpened())
            {
                using Mat frame = new Mat();
                if (videoCapture.Read(frame) && !frame.Empty())
                {
                    DetectFaces(frame, pathImage, filenameImg, ref imgIndex, format);

                    // Mostrar el frame original sin redimensionar
                    Cv2.ImShow("Frame", frame);

                    int key = Cv2.WaitKey(1);
                    if (key == 27) // Presionar ESC para salir
                        break;
                }
                else
                {
                    break;
                }
            }

            videoCapture.Release();
            Cv2.DestroyAllWindows();
        }

        private static string GetValidVideoPath()
        {
            Console.WriteLine("Por favor, ingresa la ruta del archivo de video sin el nombre del video al final: ");
            return Console.ReadLine();
        }

        private static string GetValidNameVideo()
        {
            Console.WriteLine("Ingrese el nombre del video seleccionado (con extensión): ");
            return Console.ReadLine();

        }

        private static string GetValidNameFileImage()
        {
            Console.WriteLine("Ingresa la ruta donde deseas guardar los frames: ");
            return Console.ReadLine();
        }

        private static string GetValidFileName()
        {
            Console.WriteLine("Ingresa el nombre con el que se guardará la imagen (sin extensión): ");
            string filenameImg = Console.ReadLine();

            // aqui Verificamos si el nombre del archivo contiene un punto
            return !string.IsNullOrWhiteSpace(filenameImg) && !filenameImg.Contains(".") ? filenameImg : null;
        }

        private static string GetValidFormat()
        {
            Console.Write("Ingresa el formato de archivo de la imagen (jpg, png, bmp): ");
            string format = Console.ReadLine()?.ToLower().Trim();
            HashSet<string> allowedFormats = new HashSet<string> { "jpg", "jpeg", "png", "bmp" };
            return !string.IsNullOrWhiteSpace(format) && allowedFormats.Contains(format) ? format : null;
        }


        private static void DetectFaces(Mat frame, string pathImage, string filenameImg, ref int imgIndex, string format)
        {
            using var faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");
            var faces = faceCascade.DetectMultiScale(frame, 1.1, 6, HaarDetectionTypes.ScaleImage);

            foreach (var rect in faces)
            {


                // Ajustar el rectángulo para hacerlo más alto
                Rect adjustedRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height + 20);

                // Aquí recortamos y guardamos la imagen del rostro
                using Mat faceImg = new Mat(frame, adjustedRect);

                string faceFileName = Path.Combine(pathImage, $"{filenameImg}_face_{imgIndex}.{format}");
                Cv2.ImWrite(faceFileName, faceImg);
                Console.WriteLine($"Imagen guardada como {filenameImg + "_" + imgIndex}");

                // Aquí dibujamos un rectángulo ajustado en el rostro a cada imagen que se visualiza
                Cv2.Rectangle(frame, adjustedRect, new Scalar(128, 0, 255), 2);

                // Incrementar el índice de la imagen para el próximo rostro detectado
                imgIndex++;
            }
        }
    
    }
}
