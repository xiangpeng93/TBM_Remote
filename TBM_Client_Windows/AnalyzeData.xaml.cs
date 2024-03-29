﻿using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TBM_Client_Windows
{
	/// <summary>
	/// AnalyzeData.xaml 的交互逻辑
	/// </summary>
	public partial class AnalyzeData : MetroWindow
	{
		protected class FileItem
		{
			public FileItem(string fileName)
			{
				this.m_fileName = fileName;
				
			}
			public string m_fileName{get;set;}
		};

		public class resultInfo
		{
			public string IDNumber
			{
				get;
				set;
			}
			public string keyName
			{
				get;
				set;
			}
			public int viewNum
			{
				get;
				set;
			}
			public string otherNum1
			{
				get;
				set;
			}
			public string otherNum2
			{
				get;
				set;
			}
			public string otherNum3
			{
				get;
				set;
			}
			public string otherNum4
			{
				get;
				set;
			}
			public string otherNum5
			{
				get;
				set;
			}
			public resultInfo(
							string sIdNumber,
							string sUserName,
						string sViewNum
						)
			{
				IDNumber = sIdNumber;
				keyName = sUserName;
				viewNum = Int32.Parse(sViewNum);
			}
			public resultInfo()
			{

			}
		}

		private ObservableCollection<resultInfo> m_resultInfo = new ObservableCollection<resultInfo>();
		private ObservableCollection<resultInfo> m_searchResultInfo = new ObservableCollection<resultInfo>();

		static int g_IDNumber = 0;
		private ManalWindow m_manalWindow;
		public AnalyzeData(ManalWindow manalWindow)
		{
			InitializeComponent();
			ListCollectionView cs = new ListCollectionView(m_resultInfo);
			listFileResult.ItemsSource = cs;
			ListCollectionView csSearch = new ListCollectionView(m_searchResultInfo);
			listSearchResult.ItemsSource = csSearch;

m_manalWindow = manalWindow;
		}

		private void btnSelectFiles_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xls"; // Default file extension
			dlg.Multiselect = true;
            dlg.Filter = "Excel 工作薄 (.*)|*.*"; // Filter files by extension
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
			if (result == true)
			{
				var readByFileName = m_manalWindow.getScope().GetVariable<Func<object, object>>("readByFileName");
				var getInfoListLen = m_manalWindow.getScope().GetVariable<Func<object, object>>("getInfoNumFromListInfo");
				var getInfoFromListInfo = m_manalWindow.getScope().GetVariable<Func<object, object, object, object>>("getInfoFromListInfo");
				foreach (var filename in dlg.FileNames)
				{
					Console.WriteLine(filename);
					FileItem file = new FileItem(filename.ToString());
					
					listFileListBox.Items.Add(file);
					
					var tInfoList = readByFileName(filename.ToString());

					var InfoNum = getInfoListLen(tInfoList);
					for(int i = 5; i < Int32.Parse(InfoNum.ToString()); i++)
					{
g_IDNumber++;
						resultInfo resultMsg = new resultInfo();
						resultMsg.IDNumber = g_IDNumber.ToString();
						resultMsg.keyName = getInfoFromListInfo(tInfoList, i, 0).ToString();
						try
						{
							resultMsg.viewNum = Int32.Parse(getInfoFromListInfo(tInfoList, i, 6).ToString());
						}
						catch
						{
							Console.WriteLine(getInfoFromListInfo(tInfoList, i, 6).ToString());
						}
						
						Console.WriteLine(resultMsg.keyName);
						Console.WriteLine(resultMsg.viewNum);
						m_resultInfo.Add(resultMsg);
					}
				}

			}
		}

		private void btnReSelectFiles_Click(object sender, RoutedEventArgs e)
		{
			g_IDNumber = 0;
			m_resultInfo.Clear();
			listFileListBox.Items.Clear();
		}

		private void btnStartAnalyze_Click(object sender, RoutedEventArgs e)
		{
			if (textBoxKeyName.Text.Equals(""))
			{
				return;
			}
			string[] sArray;
			if (textBoxKeyName.Text.ToString().IndexOf(',') == -1)
			{
				sArray = textBoxKeyName.Text.ToString().Split('，');
			}
			else
			{
				sArray = textBoxKeyName.Text.ToString().Split(',');
			}
			
			m_searchResultInfo.Clear();
			int iIDNumber = 0;
			foreach (string i in sArray)
			{
				Console.WriteLine(i.ToString());
				int iKeyNameNum = 0;
				foreach (var item in m_resultInfo)
				{
					if (-1 != item.keyName.ToString().IndexOf(i.ToString()))
					{
						iKeyNameNum += item.viewNum;
					}
				}
				if (iKeyNameNum > 0)
				{
					iIDNumber++;
					m_searchResultInfo.Add(new resultInfo(iIDNumber.ToString(), i, iKeyNameNum.ToString()));
				}
			}
		}

		private void btnExportData_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xls"; // Default file extension
            dlg.Filter = "Excel 工作薄 (.xls)|*.xls"; // Filter files by extension
			
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            
            // Process save file dialog box results
            if (result == true)
            {
				var initByFileName = m_manalWindow.getScope().GetVariable<Func<object,object>>("initByFileNameEx");

				var tInfoList = initByFileName(dlg.FileName);

                var insertDataByFileName = m_manalWindow.getScope().GetVariable<Func<object, object, object, object, object, object, object, object, object>>("insertInfoByFileName");

                for (int i = 0; i < m_searchResultInfo.Count; i++)
                {
                    insertDataByFileName(tInfoList, m_searchResultInfo[i].IDNumber, 
                         m_searchResultInfo[i].keyName,
                        m_searchResultInfo[i].viewNum,
                        "",
                        "",
                        "",
                        "");
                }
                var saveInfoListToFile = m_manalWindow.getScope().GetVariable<Func<object,object,object>>("wireFileByList");
				saveInfoListToFile(dlg.FileName, tInfoList);
                // Save document
                MessageBox.Show("保存成功.");
            }
            else
            {
                return;
            }
		}
		
        private bool isSortByUp = false;
		private void listFileResult_Click(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
			if (headerClicked != null  && headerClicked.Content.Equals("数量"))
			{
				if (isSortByUp)
				{
					m_resultInfo = new ObservableCollection<resultInfo>(m_resultInfo.OrderBy(p => p.viewNum));
					isSortByUp = !isSortByUp;
					for (int i = 1; i <= m_resultInfo.Count; i++)
					{
						m_resultInfo[i - 1].IDNumber = i.ToString();
					}
					ListCollectionView cs = new ListCollectionView(m_resultInfo);
					listFileResult.ItemsSource = cs;
				}
				else
				{
					m_resultInfo = new ObservableCollection<resultInfo>(m_resultInfo.OrderByDescending(p => p.viewNum));
					isSortByUp = !isSortByUp;

					for (int i = 1; i <= m_resultInfo.Count; i++)
					{
						m_resultInfo[i - 1].IDNumber = i.ToString();
					}
					ListCollectionView cs = new ListCollectionView(m_resultInfo);
					listFileResult.ItemsSource = cs;
				}
			}
		}

	}
}
