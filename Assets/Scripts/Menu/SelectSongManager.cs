﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using BMS;

using System.Collections;
using System.Collections.Generic;
using System;

public class SelectSongManager : MonoBehaviour {
    static int savedSortMode;

    public SelectSongScrollContent itemsDisplay;
    public RectTransform loadingDisplay;
    public RectTransform loadingPercentageDisplay;
    public Dropdown gameMode;
    public Dropdown colorMode;
    public Toggle autoModeToggle;
    public Dropdown judgeModeDropDown;
    public Slider speedSlider;
    public Dropdown sortMode;
    public RawImage background;
    public ColorRampLevel colorSet;
    string dataPath;

    public BMSManager bmsManager;

	void Start() {
        SongInfoLoader.CurrentCodePage = 932; // Hardcoded to Shift-JIS as most of BMS are encoded by this.
        LoadBMSInThread();
        gameMode.value = Loader.gameMode;
        colorMode.value = (int)Loader.colorMode;
        autoModeToggle.isOn = Loader.autoMode;
        judgeModeDropDown.value = Loader.judgeMode;
        speedSlider.value = Loader.speed;
        sortMode.value = savedSortMode;
        itemsDisplay.OnChangeBackground += ChangeBackground;
        itemsDisplay.OnSongInfoRemoved += SongInfoLoader.RemoveSongInfo;
    }

    void OnDestroy() {
        if(itemsDisplay != null)
            itemsDisplay.OnChangeBackground -= ChangeBackground;
    }

    public void GameModeChange(int index) {
        Loader.gameMode = index;
    }

    public void ColorModeChange(int index) {
        Loader.colorMode = (BMS.Visualization.ColoringMode)index;
    }

    public void ToggleAuto(bool state) {
        Loader.autoMode = autoModeToggle.isOn;
    }

    public void JudgeModeChange(int index) {
        Loader.judgeMode = index;
    }

    public void ChangeSpeed(float value) {
        Loader.speed = speedSlider.value;
    }

    public void ChangeSortMode(int mode) {
        savedSortMode = mode;
        switch(savedSortMode) {
            case 0: itemsDisplay.Sort(SongInfoComparer.SortMode.Name); break;
            case 1: itemsDisplay.Sort(SongInfoComparer.SortMode.Artist); break;
            case 2: itemsDisplay.Sort(SongInfoComparer.SortMode.Genre); break;
            case 3: itemsDisplay.Sort(SongInfoComparer.SortMode.Level); break;
            case 4: itemsDisplay.Sort(SongInfoComparer.SortMode.BPM); break;
        }
    }

    public void StartGame() {
        if(itemsDisplay.SelectedSongInternalIndex >= 0)
            switch(Loader.gameMode) {
                case 0: SceneManager.LoadScene("GameScene"); break;
                case 1: SceneManager.LoadScene("ClassicGameScene"); break;
            }
    }
    
    Coroutine loadBMSFilesCoroutine;
    void LoadBMSInThread() {
        loadingDisplay.gameObject.SetActive(true);
        gameObject.GetOrAddComponent(ref bmsManager);

        itemsDisplay.markLoaded = false;
        SongInfoLoader.LoadBMSInThread(bmsManager, OnLoadCacheInfo, OnAddSong);
        dataPath = Application.dataPath;
        StartCoroutine(LoadBMSEnd());
    }

    void OnAddSong(SongInfo songInfo) {
        if(itemsDisplay != null)
            itemsDisplay.AddItem(songInfo);
        else
            SongInfoLoader.StopLoadBMS();
    }

    void OnLoadCacheInfo(IEnumerable<SongInfo> songInfos) {
        if(itemsDisplay != null)
            itemsDisplay.AddItem(songInfos);
        else
            SongInfoLoader.StopLoadBMS();
    }

    IEnumerator LoadBMSEnd() {
        Vector2 loadingAnchorMax = loadingPercentageDisplay.anchorMax;
        while(SongInfoLoader.HasLoadingThreadRunning) {
            loadingAnchorMax.x = SongInfoLoader.LoadedPercentage;
            loadingPercentageDisplay.anchorMax = loadingAnchorMax;
            yield return null;
        }
        loadingAnchorMax.x = 1;
        loadingPercentageDisplay.anchorMax = loadingAnchorMax;
        loadingDisplay.gameObject.SetActive(false);
        itemsDisplay.markLoaded = true;
        itemsDisplay.Sort();
        yield break;
    }

    void ChangeBackground(Texture texture) {
        background.texture = texture;
        Color color = texture || itemsDisplay.SelectedSongInfo.index < 0 ? Color.white : colorSet.GetColor(itemsDisplay.SelectedSongInfo.level);
        color.a /= 2;
        background.color = color;
    }

}
