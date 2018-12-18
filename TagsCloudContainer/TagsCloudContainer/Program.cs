using System;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Threading;
using TagsCloudContainer.CircularCloudLayouter;
using Autofac;


namespace TagsCloudContainer
{
    class Program
    {
        private static IContainer Container { get; set; }

        // Временный пример с некоторыми настройками
        static void Main(string[] args)
        {
            var filePath = @"..\..\tmpTextFile";
            Func<Word, Size> wordSizeFunc = w => new Size(w.Count * 20 * w.Value.Length, w.Count * 20);

            var builder = new ContainerBuilder();
            builder.RegisterType<Drawer>().As<IDrawer<Word>>();
            builder.RegisterType<ItemToDraw<Rectangle>>().As<IItemToDraw<Rectangle>>();
            builder.RegisterType<Word>().As<IWord>();
            builder.RegisterType<WordStorage>().As<IWordStorage>();

            builder.RegisterType<WordsCustomizer>().AsSelf().SingleInstance();
            builder.RegisterType<RectangleStorage>().AsSelf().SingleInstance();

            var settings = new DrawSettings<Word>(filePath);
            settings.SetImageSize(new Size(1000, 500));
            settings.SetItemPainter(i => TakeRandomColor());

            builder.RegisterInstance(settings).As<DrawSettings<Word>>();

            builder.RegisterType<CircularCloudLayout>().As<IRectangleLayout>();

            builder.RegisterType<WordLayouter>()
                .WithParameter("wordStorage", new WordStorage(new WordsCustomizer()))
                .WithParameter("getWordSize", wordSizeFunc);

            Container = builder.Build();

            using (var scope = Container.BeginLifetimeScope())
            {
                var wordStorage = scope.Resolve<WordStorage>();
                var words = File.ReadAllLines(filePath + ".txt");
                wordStorage.AddRange(words);

                var layouter = scope.Resolve<WordLayouter>();

                var drawer = scope.Resolve<Drawer>();
                drawer.DrawItems(layouter.GetItemsToDraws());
            }
        }

        private static Color TakeRandomColor()
        {
            var rnd = new Random();
            Thread.Sleep(20);
            var color = Color.FromArgb(rnd.Next());

            switch (rnd.Next() % 4)
            {
                case 0:
                    color = Color.Green;
                    break;
                case 1:
                    color = Color.Red;
                    break;
                case 2:
                    color = Color.Gold;
                    break;
                case 3:
                    color = Color.Aqua;
                    break;
                default:
                    color = Color.BlueViolet;
                    break;
            }

            return color;
        }
    }
}
