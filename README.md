# Create Chamfer
*Аддон для Revit 2022 (на других не тестировался)*

## Технологии
C# и .NET 4.8

## Описание
Принцип работы заключается в выделении 4 стен, и построении тополгии с привязкой к уровню стен.
По умолчанию создается прямое пространство на уровне 0. Далее считываются данные о выделенных сденах и об их уровне.

**ВАЖНО!** Если стены стоят с отрицательным смещением, но на уровне 0, то программа выдаст просто прямое пространство.

## Установка
1. Перенесите [файл с собранным проектом](dll/ClassLibrary1.dll) в удобное для использования место.
2. Перенесите [файл с конфигурацией аддона](xml/AddInManifest.addin) в папку, подготовленную Revit для хранения аддонов. По умолчанию это ```C:\ProgramData\Autodesk\Revit\Addins\20xx\```.
3. Измените в [файле с конфигурацией аддона](xml/AddInManifest.addin) поле Assembly, согласно пути, по которому вы расположили файл в пункте 1. Все остальные значения следует оставить по умолчанию:

```
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
 <AddIn Type="Command">
       <!--...-->
       <Assembly><!--ПУТЬ К ФАЙЛУ ClassLibrary1.dll--></Assembly>
       <!--...-->
 </AddIn>
</RevitAddIns>
```

## Использование 
1. Откройте проект и выделите 4 стены, образующие прямоугольный контур. (~~Уровень которых не совпадает с 0, иначе эффекта не будет~~)
2. Перейдите на вкладку "Надстройки" и нажмите на кнопку "Внешние инструменты". Далее, если установку вы выполнили верно, появится выпадающий список, включающий название плагина.
3. Нажмите на название плагина и наслаждайтесь результатом.