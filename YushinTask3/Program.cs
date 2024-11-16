using Newtonsoft.Json;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Text.Json.Serialization.Metadata;
using System.Globalization;

class Program
{
    static void Main()
    {   //Путь к файлу file.json
        string fileJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "file.json");
        //Пытаемся открыть file.json
        try
        { 
            //Десереализуем json в словарь вида ключ(object_xxx) : значение (все остальное в фигурных скобках, используем для этого отдельный класс) чтобы можно было работать с содержимым
            var fileObjects = JsonConvert.DeserializeObject<Dictionary<string, fileJsonObjects>>(File.ReadAllText(fileJsonPath));


            //Путь к файлу task.json
            string taskJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "task.json");

            //Пытаемся открыть task.json
            try
            {
                //Десереализуем json в словарь(вида ключ(list_xxxx): значение(храним в классе) чтобы можно было работать с содержимым
                var taskObjects = JsonConvert.DeserializeObject<Dictionary<string, taskJsonObjects>>(File.ReadAllText(taskJsonPath));

                //Создаем словарь объектов для сохранения результата
                var newFileObjects = new Dictionary<string, fileJsonObjects>();
                //Задаем путь к файлу items.csv (при добавлении файл items.csv не отображался, пришлось переименовать
                string itemCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "items_1.csv");
                //перебираем ключи fileObjects
                foreach (var fileObjectKey in fileObjects.Keys)
                {
                    //временная переменная для удобства
                    var fileObj = fileObjects[fileObjectKey];

                    //перебираем ключи taskObjectKey
                    foreach (var taskObjectKey in taskObjects.Keys)
                    {
                        //Если список (доска задач) включает в себя ключ объекта из fileObject(то бишь имя задачи)
                        if (taskObjects[taskObjectKey].List.Contains(fileObjectKey))
                        {
                            if (!newFileObjects.ContainsKey(fileObjectKey))// и проверяем его уникальность
                            {
                                newFileObjects.Add(fileObjectKey, fileObj);//добавляем в новый словарь
                            }
                            //newFileObjects[fileObjectKey] = fileObj;
                        }

                    }
                }

                Console.WriteLine("New file objects:");
                foreach (var obj in newFileObjects.Values) //проходимся по записям VALUE из словаря
                {
                    foreach (var line in File.ReadLines(itemCsvPath)) // построчно считываем item_1.csv
                    {
                        var col = line.Split(','); //используемые разделители
                        string rew = col[0]; //название награды
                        if (obj.reward == rew) //если название награды совпадает с item_1.csv, то
                        {//добаляем оставшуюся информацию о наградах
                            obj.money = int.Parse(col[1]);
                            obj.details = int.Parse(col[2]);
                            obj.reputation = int.Parse(col[3]);
                        }

                    }
                }
                string newFileJson = JsonConvert.SerializeObject(newFileObjects, Formatting.Indented);//сереализуем объект в json
                Console.WriteLine(newFileJson);//для проверки вывода
                string resultJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "file1.json");
                if (File.Exists(resultJsonPath))
                { //если файл существует то выводим путь
                    Console.WriteLine($"Файл найден: {resultJsonPath}");
                }
                else
                {// иначе выводим что файл не найден
                    Console.WriteLine($"Файл не найден: {resultJsonPath}");
                }
                //записываем в файл
                File.WriteAllText(resultJsonPath, newFileJson);

                //создает список объектов excelObjects
                List<newExcelObject> excelObjects = new List<newExcelObject>();
                foreach (var taskObjectKey in taskObjects.Keys)
                {
                    var taskObj = taskObjects[taskObjectKey]; 
                    foreach (var fileObjectKey in fileObjects.Keys) 
                    {
                        newExcelObject exObject = new newExcelObject() // создаем экземляр объекта
                        {
                            objectInfo = new fileJsonObjects() // Инициализируем objectInfo
                        };
                        var fileObj = fileObjects[fileObjectKey];
                        bool bObjectFilled = false; //Переменная для проверки заполнился ли объект exObject
                        foreach (var line in File.ReadLines(itemCsvPath)) // построчно считываем item_1.csv
                        {
                            var col = line.Split(','); //используемые разделители
                            string rew = col[0]; //название награды
                            if (fileObj.reward == rew) //если название награды совпадает с item_1.csv, то
                            {//добаляем оставшуюся информацию о наградах

                                exObject.list_name = taskObjectKey;
                                exObject.object_name = fileObjectKey;
                                exObject.objectInfo.reward = rew;
                                exObject.objectInfo.money = int.Parse(col[1]);
                                exObject.objectInfo.details = int.Parse(col[2]);
                                exObject.objectInfo.reputation = int.Parse(col[3]);
                                //Проверяем содержится ли имя задачи в file.json
                                if (fileObjects.ContainsKey(exObject.object_name))
                                {
                                    exObject.isUsed = 1;
                                }
                                else exObject.isUsed = 0;
                                bObjectFilled = true;
                                break;
                            }
                        }
                        //Проверка на то заполнились ли поля
                        if (bObjectFilled)
                        {
                            //Если заполнились, то добавляем объект в список
                            excelObjects.Add(exObject);
                        }

                    }

                }
                //СОртируем по list_name
                var sortedExcelObjects = excelObjects.OrderBy(exObjects => exObjects.list_name);
                //Путь для нового csv
                string newCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "excel.csv");
                //Инструменты для записи csv
                using (var writer = new StreamWriter(newCsvPath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // Записываем заголовки
                    csv.WriteHeader<newExcelObject>();
                    csv.NextRecord();
                    //Записываем отсортированные данные
                    foreach (var sortedExcelObject in sortedExcelObjects)
                    {
                        csv.WriteRecord(sortedExcelObject);
                        csv.NextRecord();
                    }
                }

                Console.WriteLine("Данные успешно записаны в CSV.");



            }//обработка ошибок при десериализации файлов
            catch (Exception exception2)
            {
                Console.WriteLine($"Ошибка при десериализации task.json : {exception2.Message}");
            }
        }
        catch (Exception exception1)
        {
            Console.WriteLine($"Ошибка при десериализации file.json : {exception1.Message}");
        }
    }
}
//класс для обработки Задач
public class fileJsonObjects
{

    public string reward { get; set; }
    public int money { get; set; }
    public int details { get; set; }

    public int reputation { get; set; }
}
//класс для обработки Досок
public class taskJsonObjects
{

    public List<string> List { get; set; }

}

public class newExcelObject
{
    public string list_name { get; set; }

    public string object_name { get; set; }
    public fileJsonObjects objectInfo { get; set; }
    public int isUsed { get; set; }

}
