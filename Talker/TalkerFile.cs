using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Talker
{
	public class TalkerFile
	{
		public TalkerFile()
		{
		}

		public static string GetFile(string fileName, string section = "")
		{
			string fileContents = "";
			
			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("SELECT contents FROM talkerFiles WHERE fileName = ?fileName", conn)) {
					cmd.Parameters.AddWithValue("fileName", fileName);
					//TODO: add section?
					
					cmd.Connection.Open();

					object result = cmd.ExecuteScalar();

					if(DBNull.Value != result) {
						fileContents = (string)result;
					}
					
					cmd.Connection.Close();
				}
			}
			
			return fileContents;
		}

		public static List<TalkerFile> GetFiles(string section)
		{
			List<TalkerFile> talkerFiles = new List<TalkerFile>();

			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("SELECT fileId, fileName, contents, section, description FROM talkerFiles WHERE section = ?section ORDER BY fileName", conn)) {
					cmd.Parameters.AddWithValue("section", section);
					
					cmd.Connection.Open();

					MySqlDataReader dataReader = cmd.ExecuteReader();
					TalkerFile currentTalker;

					while(dataReader.Read()) {
						currentTalker = new TalkerFile();

						currentTalker.FileId = long.Parse(dataReader["fileId"].ToString());
						currentTalker.FileName = dataReader["fileName"].ToString();
						currentTalker.Contents = dataReader["contents"].ToString();
						currentTalker.Section = dataReader["section"].ToString();
						currentTalker.Description = dataReader["description"].ToString();

						talkerFiles.Add(currentTalker);
					}
					
					cmd.Connection.Close();
				}
			}

			return talkerFiles;
		}

		public long FileId {
			get;
			set;
		}

		public string FileName {
			get;
			set;
		}

		public string Contents {
			get;
			set;
		}

		public string Section {
			get;
			set;
		}

		public string Description {
			get;
			set;
		}
	}
}

