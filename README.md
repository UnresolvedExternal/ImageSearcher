# ImageSearcher

UI for control of set of image patterns. API for searching subpictures.

Example of usage:
````csharp
void PrintBananasPositions(Bitmap plentyOfFruits)
{
	var templateStore = BitmapStore.Load("C:\\fruitsTemplates.store");

	foreach (var position in templateStore.GetPositions("banana"))
		Console.WriteLine("Banana found at {0}", position);
}
````