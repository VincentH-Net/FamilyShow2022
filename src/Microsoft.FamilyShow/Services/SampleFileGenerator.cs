using System;
using System.Drawing;
using System.IO;
using Microsoft.FamilyShow.Properties;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Services;

public class SampleFileGenerator
{
    /// <summary>
    ///     Install sample files the first time the application runs.
    /// </summary>
    public static void InstallSampleFiles(string applicationFolderName)
    {
        // Full path to the document file location.
        var location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), applicationFolderName);

        // Return right away if the data file already exist.
        if (Directory.Exists(location)) return;

        try
        {
            // Sample data files.
            Directory.CreateDirectory(location);
            CreateSampleFile(location, "Windsor.family", Resources.WindsorSampleFile);
            CreateSampleFile(location, "Windsor0.family", Resources.WindsorSampleFile);
            CreateSampleFile(location, "Windsor1.family", Resources.WindsorSampleFile);
            CreateSampleFile(location, "Windsor2.family", Resources.WindsorSampleFile);
            CreateSampleFile(location, "Kennedy.ged", Resources.KennedySampleFile);

            // Sample image files.
            var imageLocation = Path.Combine(location, Photo.Const.PhotosFolderName);
            Directory.CreateDirectory(imageLocation);
            CreateSampleFile(imageLocation, "Edward VII.jpg", Resources.Image_Edward_VII);
            CreateSampleFile(imageLocation, "Edward VIII.jpg", Resources.Image_Edward_VIII);
            CreateSampleFile(imageLocation, "Elizabeth II.jpg", Resources.Image_Elizabeth_II);
            CreateSampleFile(imageLocation, "George V.jpg", Resources.Image_George_V);
            CreateSampleFile(imageLocation, "George VI.jpg", Resources.Image_George_VI);
            CreateSampleFile(imageLocation, "Margaret Rose.jpg", Resources.Image_Margaret_Rose);
            CreateSampleFile(imageLocation, "Prince Charles.jpg", Resources.Image_Prince_Charles);
            CreateSampleFile(imageLocation, "Prince Henry.jpg", Resources.Image_Prince_Henry);
            CreateSampleFile(imageLocation, "Prince William.jpg", Resources.Image_Prince_William);
            CreateSampleFile(imageLocation, "Princess Diana.jpg", Resources.Image_Princess_Diana);

            // Sample strory files.
            var storyLocation = Path.Combine(location, Story.Const.StoriesFolderName);
            Directory.CreateDirectory(storyLocation);
            CreateSampleFile(storyLocation, "Camilla Shand {cb2c1f69-5311-403a-948f-eaf74dd8e72d}.rtf", Resources.Story_Camilla_Shand);
            CreateSampleFile(storyLocation, "Edward VII Wettin {I1}.rtf", Resources.Story_Edward_VII_Wettin);
            CreateSampleFile(storyLocation, "Edward VIII Windsor {I5}.rtf", Resources.Story_Edward_VIII_Windsor);
            CreateSampleFile(storyLocation, "Elizabeth II Alexandra Mary Windsor {I9}.rtf", Resources.Story_Elizabeth_II_Alexandra_Mary_Windsor);
            CreateSampleFile(storyLocation, "George V Windsor {I3}.rtf", Resources.Story_George_V_Windsor);
            CreateSampleFile(storyLocation, "George VI Windsor {I7}.rtf", Resources.Story_George_VI_Windsor);
            CreateSampleFile(storyLocation, "Margaret Rose Windsor {I24}.rtf", Resources.Story_Margaret_Rose_Windsor);
            CreateSampleFile(storyLocation, "Charles Philip Arthur Windsor {I11}.rtf", Resources.Story_Charles_Philip_Arthur_Windsor);
            CreateSampleFile(storyLocation, "Diana Frances Spencer {I12}.rtf", Resources.Story_Diana_Frances_Spencer);
        }
        catch
        {
            // Could not install the sample files, handle all exceptions the same
            // ignore and continue without installing the sample files.
        }
    }

    /// <summary>
    ///     Extract the sample family files from the executable and write it to the file system.
    /// </summary>
    private static void CreateSampleFile(string location, string fileName, byte[] fileContent)
    {
        // Full path to the sample file.
        var path = Path.Combine(location, fileName);

        // Return right away if the file already exists.
        if (File.Exists(path)) return;

        // Create the file.
        using var writer = new BinaryWriter(File.Open(path, FileMode.Create));
        writer.Write(fileContent);
    }

    private static void CreateSampleFile(string location, string fileName, string fileContent)
    {
        // Full path to the sample file.
        var path = Path.Combine(location, fileName);

        // Return right away if the file already exists.
        if (File.Exists(path)) return;

        // Create the file.
        using var writer = new StreamWriter(File.Open(path, FileMode.Create));
        writer.Write(fileContent);
    }

    private static void CreateSampleFile(string location, string fileName, Bitmap image)
    {
        // Full path to the sample file.
        var path = Path.Combine(location, fileName);

        // Return right away if the file already exists.
        if (File.Exists(path)) return;

        // Create the file.
        image.Save(path);
    }
}