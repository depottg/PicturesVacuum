﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using Unity.Collections;
using Unity.Jobs;
using System.Threading;
using System.IO;

// TODO Error log
// TODO Animations
// TODO Stop during job execution
// TODO Add try/catch everywhere

public class ProgramManager : MonoBehaviour
{
    // Inputs and outputs

    public InputField srcInput;
    public InputField dstInput;
    public InputField extensionsInput;
    public Button scanButton;
    public Button defaultButton;
    public Button stopButton;

    // Job management

    NativeArray<char> jobSrcPathAttribute;
    NativeArray<char> jobDstPathAttribute;
    NativeArray<char> jobExtensionsAttribute;
    private JobHandle jobHandle;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (jobHandle.IsCompleted)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            scanButton.interactable = true;
            defaultButton.interactable = true;
            stopButton.interactable = false;
            EndJob();
        }
        else
        {
            scanButton.interactable = false;
            defaultButton.interactable = false;
            stopButton.interactable = true;
        }
    }

    public void OpenScrFolder()
    {
        string path = FileBrowser.OpenSingleFolder();
        srcInput.text = path;
    }

    public void OpenDstFolder()
    {
        string path = FileBrowser.OpenSingleFolder();
        dstInput.text = path;
    }

    public void ScanAndCopy()
    {
        // Checks that the source path is empty or leads to an actual directory
        if (srcInput.text != "" && !Directory.Exists(srcInput.text))
        {
            ProgressionDisplayManager.instance.SetErrorCode(1);
            return;
        }
        // Checks that the destination is an existing directory
        if (dstInput.text == "" || !Directory.Exists(dstInput.text))
        {
            ProgressionDisplayManager.instance.SetErrorCode(2);
            return;
        }
        // Checks that the extensions list is not empty
        if (extensionsInput.text.Length == 0)
        {
            ProgressionDisplayManager.instance.SetErrorCode(3);
            return;
        }
        ProgressionDisplayManager.instance.SetErrorCode(0);

        // Creating the attributes arrays
        jobSrcPathAttribute = new NativeArray<char>(srcInput.text.Length, Allocator.Persistent);
        for (int i = 0; i < srcInput.text.Length; i++)
            jobSrcPathAttribute[i] = srcInput.text[i];
        jobDstPathAttribute = new NativeArray<char>(dstInput.text.Length, Allocator.Persistent);
        for (int i = 0; i < dstInput.text.Length; i++)
            jobDstPathAttribute[i] = dstInput.text[i];
        jobExtensionsAttribute = new NativeArray<char>(extensionsInput.text.Length, Allocator.Persistent);
        for (int i = 0; i < extensionsInput.text.Length; i++)
            jobExtensionsAttribute[i] = extensionsInput.text[i];

        // Creating the job
        ScanAndCopyJob jobData = new ScanAndCopyJob();
        jobData.jobSrcPathAttribute = jobSrcPathAttribute;
        jobData.jobDstPathAttribute = jobDstPathAttribute;
        jobData.jobExtensionsAttribute = jobExtensionsAttribute;

        // Scheduling the job
        jobHandle = jobData.Schedule();
    }

    public void EndJob()
    {
        // Handling the job
        jobHandle.Complete();

        // Disposing the communication array
        if (jobSrcPathAttribute.Length > 0)
        {
            jobSrcPathAttribute.Dispose();
            jobDstPathAttribute.Dispose();
            jobExtensionsAttribute.Dispose();
        }
    }

    public void OnDestroy()
    {
        EndJob();
    }
    public void LoadDefault()
    {
        extensionsInput.text = "jpeg jpg JPG png PNG gif mp4 mov avi";
    }
}
