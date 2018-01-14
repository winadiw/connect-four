using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ConnectFour
{
	public class GameController : MonoBehaviour 
	{
		enum Piece
		{
			Empty = 0,
			Blue = 1,
			Red = 2
		}

		[Range(3, 8)]
		public int numRows = 6;
		[Range(3, 8)]
		public int numColumns = 7;

		//private int[][] board = new int[6][];
		private int[,] board = new int[6,7]{
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 }
		};

		private int[,] bestMove = new int[1,2]{
			{ -1, -1 }
		};
		private int step = 0;
		private int maxDepth = 7;
		[Tooltip("How many pieces have to be connected to win.")]
		public int numPiecesToWin = 4;

		[Tooltip("Allow diagonally connected Pieces?")]
		public bool allowDiagonally = true;

		public float dropTime = 4f;

		private float elapsedTime = 0f;

		bool isPlayerFirst;

		bool depthShown;

		DateTime currentTime;

		// Gameobjects 
		public GameObject pieceRed;
		public GameObject pieceBlue;
		public GameObject pieceField;

		public GameObject winningText;
		public string playerWonText = "You Won!";
		public string playerLoseText = "You Lose!";
		public string drawText = "Draw!";

		//public Text maxDepthText;
		//public Text stepText;

		public GameObject btnPlayAgain;
		bool btnPlayAgainTouching = false;
		Color btnPlayAgainOrigColor;
		Color btnPlayAgainHoverColor = new Color(255, 143,4);

		GameObject gameObjectField;

		// temporary gameobject, holds the piece at mouse position until the mouse has clicked
		GameObject gameObjectTurn;

		/// <summary>
		/// The Game field.
		/// 0 = Empty
		/// 1 = Blue
		/// 2 = Red
		/// </summary>
		int[,] field;

		bool isPlayersTurn = true;
		bool isLoading = true;
		bool isDropping = false; 
		bool mouseButtonPressed = false;

		bool gameOver = false;
		bool isCheckingForWinner = false;

		// Use this for initialization
		void Start () 
		{
			/*
			{
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0 }
			};

			bestMove [0] = new int[]{ -1, -1 };

			for(int i = 0; i < 6; i++)
			{
				board [i] = new int[] { 0, 0, 0, 0, 0, 0, 0 };
			}
			*/
			int max = Mathf.Max (numRows, numColumns);

			if(numPiecesToWin > max)
				numPiecesToWin = max;

			CreateField ();

			//isPlayersTurn = System.Convert.ToBoolean(Random.Range (0, 1));
			isPlayerFirst = GetTurn();
			if(isPlayerFirst)
				isPlayersTurn = true;
			else
				isPlayersTurn = false;
			btnPlayAgainOrigColor = btnPlayAgain.GetComponent<Renderer>().material.color;
			/*
			maxDepthText = GetComponent<Text>();
			*/
			//stepText = GetComponent<Text>();
		}

		bool GetTurn()
		{
			String turn = PlayerPrefs.GetString ("FirstTurn");
			if (turn == "Player")
				return true;
			else
				return false;
		}

		/// <summary>
		/// Creates the field.
		/// </summary>
		void CreateField()
		{
			winningText.SetActive(false);
			btnPlayAgain.SetActive(false);

			isLoading = true;

			gameObjectField = GameObject.Find ("Field");
			if(gameObjectField != null)
			{
				DestroyImmediate(gameObjectField);
			}
			gameObjectField = new GameObject("Field");

			// create an empty field and instantiate the cells
			field = new int[numColumns, numRows];
			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					field[x, y] = (int)Piece.Empty;
					GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
					g.transform.parent = gameObjectField.transform;
				}
			}

			isLoading = false;
			gameOver = false;

			// center camera
			Camera.main.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f), Camera.main.transform.position.z);

			winningText.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) + 1, winningText.transform.position.z);

			btnPlayAgain.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) - 1, btnPlayAgain.transform.position.z);
		}

		/// <summary>
		/// Spawns a piece at mouse position above the first row
		/// </summary>
		/// <returns>The piece.</returns>
		GameObject SpawnPiece()
		{
			Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			step++;
			//Debug.Log ("Step:" + step);


			if (step % 7 == 0) maxDepth++;

			//Debug.Log ("Max Depth: " + maxDepth);

			if(!isPlayersTurn)
			{
				List<int> moves = GetPossibleMoves();

				if(moves.Count > 0)
				{
					
					//int column = moves[Random.Range (0, moves.Count)];
					int i;
					if(isPlayerFirst)
					{
						currentTime = DateTime.Now;
						i = NegaMax (board, 0, -1, -1000000, 1000000);
					}
						
					else
					{
						currentTime = DateTime.Now;
						i = NegaMax (board, 0, 1, -1000000, 1000000);
					}

					depthShown = false;
					//Debug.Log ((DateTime.Now - currentTime).TotalSeconds);
					//Debug.Log(elapsedTime);
					//elapsedTime = 0f;
					//Debug.Log ("NegaMax: " + i);
					int column = bestMove[0,1];

					spawnPos = new Vector3(column, 0, 0);
				}
			}

			GameObject g = Instantiate(
				isPlayersTurn ? pieceBlue : pieceRed, // is players turn = spawn blue, else spawn red
				new Vector3(
					Mathf.Clamp(spawnPos.x, 0, numColumns-1), 
					gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
				Quaternion.identity) as GameObject;

			return g;
		}

		void UpdatePlayAgainButton()
		{
			RaycastHit hit;
			//ray shooting out of the camera from where the mouse is
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit) && hit.collider.name == btnPlayAgain.name)
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					btnPlayAgainTouching = true;

					//CreateField();
					Application.LoadLevel(0);
				}
			}
			else
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}

			if(Input.touchCount == 0)
			{
				btnPlayAgainTouching = false;
			}
		}

		// Update is called once per frame
		void Update () 
		{
			//stepText.text = "Step:" + step.ToString();
			//maxDepthText.text = "Max Depth: " + maxDepth.ToString();
			if(isLoading)
				return;

			if(isCheckingForWinner)
				return;

			if(gameOver)
			{
				winningText.SetActive(true);
				btnPlayAgain.SetActive(true);

				UpdatePlayAgainButton();

				return;
			}

			if(isPlayersTurn)
			{
				if(gameObjectTurn == null)
				{
					gameObjectTurn = SpawnPiece();
				}
				else
				{

					// update the objects position
					Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					gameObjectTurn.transform.position = new Vector3(
						Mathf.Clamp(pos.x, 0, numColumns-1), 
						gameObjectField.transform.position.y + 1, 0);

					// click the left mouse button to drop the piece into the selected column
					if(Input.GetMouseButtonDown(0) && !mouseButtonPressed && !isDropping)
					{
						mouseButtonPressed= true;

						StartCoroutine(dropPiece(gameObjectTurn));
					}
					else
					{
						mouseButtonPressed = false;
					}
				}
			}
			else
			{
				if(gameObjectTurn == null)
				{
					gameObjectTurn = SpawnPiece();
				}
				else
				{
					if(!isDropping)
						StartCoroutine(dropPiece(gameObjectTurn));
				}
			}
		}

		/// <summary>
		/// Gets all the possible moves.
		/// </summary>
		/// <returns>The possible moves.</returns>
		public List<int> GetPossibleMoves()
		{
			List<int> possibleMoves = new List<int>();
			for (int x = 0; x < numColumns; x++)
			{
				for(int y = numRows - 1; y >= 0; y--)
				{
					if(field[x, y] == (int)Piece.Empty)
					{
						possibleMoves.Add(x);
						break;
					}
				}
			}
			return possibleMoves;
		}

		/// <summary>
		/// This method searches for a empty cell and lets 
		/// the object fall down into this cell
		/// </summary>
		/// <param name="gObject">Game Object.</param>
		IEnumerator dropPiece(GameObject gObject)
		{
			isDropping = true;

			Vector3 startPosition = gObject.transform.position;
			Vector3 endPosition = new Vector3();

			// round to a grid cell
			int x = Mathf.RoundToInt(startPosition.x);
			startPosition = new Vector3(x, startPosition.y, startPosition.z);

			// is there a free cell in the selected column?
			bool foundFreeCell = false;
			for(int i = numRows-1; i >= 0; i--)
			{
				if(field[x, i] == 0)
				{
					foundFreeCell = true;
					field[x, i] = isPlayersTurn ? (int)Piece.Blue : (int)Piece.Red;
					board [i,x] = isPlayersTurn ? -1 : 1;
					endPosition = new Vector3(x, i * -1, startPosition.z);

					break;
				}
			}
			/*
			for(int i = 0; i < numRows; i++)
			{
				int j = 0;
				while(j < numColumns)
				{
					Debug.Log (board [i,j] + " " + i + " " + j);
					++j;
				}
			}
			for(int i = 0; i < numColumns; i++)
			{
				int j = 0;
				while(j < numRows)
				{
					Debug.Log (field [i,j] + " " + i + " " + j);
					++j;
				}
			}
			*/
			if(foundFreeCell)
			{
				// Instantiate a new Piece, disable the temporary
				GameObject g = Instantiate (gObject) as GameObject;
				gameObjectTurn.GetComponent<Renderer>().enabled = false;

				float distance = Vector3.Distance(startPosition, endPosition);

				float t = 0;
				while(t < 1)
				{
					t += Time.deltaTime * dropTime * ((numRows - distance) + 1);

					g.transform.position = Vector3.Lerp (startPosition, endPosition, t);
					yield return null;
				}

				g.transform.parent = gameObjectField.transform;

				// remove the temporary gameobject
				DestroyImmediate(gameObjectTurn);

				// run coroutine to check if someone has won
				StartCoroutine(Won());

				// wait until winning check is done
				while(isCheckingForWinner)
					yield return null;

				isPlayersTurn = !isPlayersTurn;
			}

			isDropping = false;

			yield return 0;
		}

		/// <summary>
		/// Check for Winner
		/// </summary>
		IEnumerator Won()
		{
			isCheckingForWinner = true;

			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					// Get the Laymask to Raycast against, if its Players turn only include
					// Layermask Blue otherwise Layermask Red
					int layermask = isPlayersTurn ? (1 << 8) : (1 << 9);

					// If its Players turn ignore red as Starting piece and wise versa
					if(field[x, y] != (isPlayersTurn ? (int)Piece.Blue : (int)Piece.Red))
					{
						continue;
					}

					// shoot a ray of length 'numPiecesToWin - 1' to the right to test horizontally
					RaycastHit[] hitsHorz = Physics.RaycastAll(
						new Vector3(x, y * -1, 0), 
						Vector3.right, 
						numPiecesToWin - 1, 
						layermask);

					// return true (won) if enough hits
					if(hitsHorz.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// shoot a ray up to test vertically
					RaycastHit[] hitsVert = Physics.RaycastAll(
						new Vector3(x, y * -1, 0), 
						Vector3.up, 
						numPiecesToWin - 1, 
						layermask);

					if(hitsVert.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// test diagonally
					if(allowDiagonally)
					{
						// calculate the length of the ray to shoot diagonally
						float length = Vector2.Distance(new Vector2(0, 0), new Vector2(numPiecesToWin - 1, numPiecesToWin - 1));

						RaycastHit[] hitsDiaLeft = Physics.RaycastAll(
							new Vector3(x, y * -1, 0), 
							new Vector3(-1 , 1), 
							length, 
							layermask);

						if(hitsDiaLeft.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}

						RaycastHit[] hitsDiaRight = Physics.RaycastAll(
							new Vector3(x, y * -1, 0), 
							new Vector3(1 , 1), 
							length, 
							layermask);

						if(hitsDiaRight.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}
					}

					yield return null;
				}

				yield return null;
			}

			// if Game Over update the winning text to show who has won
			if(gameOver == true)
			{
				winningText.GetComponent<TextMesh>().text = isPlayersTurn ? playerWonText : playerLoseText;
			}
			else 
			{
				// check if there are any empty cells left, if not set game over and update text to show a draw
				if(!FieldContainsEmptyCell())
				{
					gameOver = true;
					winningText.GetComponent<TextMesh>().text = drawText;
				}
			}

			isCheckingForWinner = false;

			yield return 0;
		}

		/// <summary>
		/// check if the field contains an empty cell
		/// </summary>
		/// <returns><c>true</c>, if it contains empty cell, <c>false</c> otherwise.</returns>
		bool FieldContainsEmptyCell()
		{
			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					if(field[x, y] == (int)Piece.Empty)
						return true;
				}
			}
			return false;
		}

		int analysis() 
		{
			int whoWon = 0;


				// Horizontal Player One
				for(int col = 0;col <= 3;col++) 
				{
					for(int row = 0;row < 6;row++) 
					{
						if(board[row,col] == 1 && board[row,col + 1] == 0 && board[row,col + 2] == 0 && board[row,col + 3] == 0) 
							whoWon = whoWon + 100; //125 -
						else if(board[row,col] == 0 && board[row,col + 1] == 1 && board[row,col + 2] == 0 && board[row,col + 3] == 0) 
							whoWon = whoWon + 100;
						else if(board[row,col] == 0 && board[row,col + 1] == 0 && board[row,col + 2] == 1 && board[row,col + 3] == 0) 
							whoWon = whoWon + 100;
						else if(board[row,col] == 0 && board[row,col + 1] == 0 && board[row,col + 2] == 0 && board[row,col + 3] == 1) 
							whoWon = whoWon + 100;
						else if(board[row,col] == 1 && board[row,col + 1] == 1 && board[row,col + 2] == 0 && board[row,col + 3] == 0) 
							whoWon = whoWon + 1000; //250 +
						else if(board[row,col] == 0 && board[row,col + 1] == 1 && board[row,col + 2] == 1 && board[row,col + 3] == 0) 
							whoWon = whoWon + 1000;
						else if(board[row,col] == 0 && board[row,col + 1] == 0 && board[row,col + 2] == 1 && board[row,col + 3] == 1) 
							whoWon = whoWon + 1000;
						else if(board[row,col] == 1 && board[row,col + 1] == 0 && board[row,col + 2] == 1 && board[row,col + 3] == 0) 
							whoWon = whoWon + 1000;
						else if(board[row,col] == 0 && board[row,col + 1] == 1 && board[row,col + 2] == 0 && board[row,col + 3] == 1) 
							whoWon = whoWon + 1000;
						else if(board[row,col] == 1 && board[row,col + 1] == 0 && board[row,col + 2] == 0 && board[row,col + 3] == 1) 
							whoWon = whoWon + 1000;
						else if(board[row,col] == 1 && board[row,col + 1] == 1 && board[row,col + 2] == 1 && board[row,col + 3] == 0) 
							whoWon = whoWon + 10000;//1000 -
						else if(board[row,col] == 0 && board[row,col + 1] == 1 && board[row,col + 2] == 1 && board[row,col + 3] == 1) 
							whoWon = whoWon + 10000;
						else if(board[row,col] == 1 && board[row,col + 1] == 0 && board[row,col + 2] == 1 && board[row,col + 3] == 1) 
							whoWon = whoWon + 10000;
						else if(board[row,col] == 1 && board[row,col + 1] == 1 && board[row,col + 2] == 0 && board[row,col + 3] == 1) 
							whoWon = whoWon + 10000;
						else if(board[row,col] == 1 && board[row,col + 1] == 1 && board[row,col + 2] == 1 && board[row,col + 3] == 1) 
							whoWon = whoWon + 100000;
					}
				}
				//Horizontal Player One End

				//Vertical Player One
				for(int col = 0; col < 7; col++)
				{
					for(int row = 5; row >= 3; row--)
					{
						if (board [row, col] == 1 && board [row - 1, col] == 0 && board [row - 2, col] == 0 && board [row - 3, col] == 0)
							whoWon = whoWon + 100;
						else if (board [row, col] == 1 && board [row - 1, col] == 1 && board [row - 2, col] == 0 && board [row - 3, col] == 0)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 1 && board [row - 1, col] == 1 && board [row - 2, col] == 1 && board [row - 3, col] == 0)
							whoWon = whoWon + 10000;
						else if (board [row, col] == 1 && board [row - 1, col] == 1 && board [row - 2, col] == 1 && board [row - 3, col] == 1)
							whoWon = whoWon + 100000;
					}
				}
				//Vertical Player One End

				//Diagonal Player One
				for(int col = 0; col < 4; col++)
				{
					for(int row = 5; row >=3 ; row--)
					{
						if (board [row, col] == 1 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 0)
							whoWon = whoWon + 100;
						else if (board [row, col] == 1 && board [row - 1, col + 1] == 1 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 0)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 1 && board [row - 1, col + 1] == 1 && board [row - 2, col + 2] == 1 && board [row - 3, col + 3] == 0)
							whoWon = whoWon + 10000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 1 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 0)
							whoWon = whoWon + 100;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 1 && board [row - 2, col + 2] == 1 && board [row - 3, col + 3] == 0)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 1 && board [row - 2, col + 2] == 1 && board [row - 3, col + 3] == 1)
							whoWon = whoWon + 10000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 1 && board [row - 3, col + 3] == 0)
							whoWon = whoWon + 100;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 1 && board [row - 3, col + 3] == 1)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 1)
							whoWon = whoWon + 100;
						else if (board [row, col] == 1 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 1 && board [row - 3, col + 3] == 0)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 1 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 1)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 1 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 1)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 1 && board [row - 1, col + 1] == 1 && board [row - 2, col + 2] == 1 && board [row - 3, col + 3] == 1)
							whoWon = whoWon + 100000;
					}
				}

				for(int col = 6; col >= 3; col--)
				{
					for(int row = 5; row >=3 ; row--)
					{
						if (board [row, col] == 1 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 0)
							whoWon = whoWon + 100;
						else if (board [row, col] == 1 && board [row - 1, col - 1] == 1 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 0)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 1 && board [row - 1, col - 1] == 1 && board [row - 2, col - 2] == 1 && board [row - 3, col - 3] == 0)
							whoWon = whoWon + 10000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 1 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 0)
							whoWon = whoWon + 100;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 1 && board [row - 2, col - 2] == 1 && board [row - 3, col - 3] == 0)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 1 && board [row - 2, col - 2] == 1 && board [row - 3, col - 3] == 1)
							whoWon = whoWon + 10000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 1 && board [row - 3, col - 3] == 0)
							whoWon = whoWon + 100;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 1 && board [row - 3, col - 3] == 1)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 1)
							whoWon = whoWon + 100;
						else if (board [row, col] == 1 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 1 && board [row - 3, col - 3] == 0)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 1 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 1)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 1 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 1)
							whoWon = whoWon + 1000;
						else if (board [row, col] == 1 && board [row - 1, col - 1] == 1 && board [row - 2, col - 2] == 1 && board [row - 3, col - 3] == 1)
							whoWon = whoWon + 100000;
					}
				}
				//Diagonal Player One End



				// Horizontal Player Two
				for(int col = 0;col <= 3;col++) {
					for(int row = 0;row < 6;row++) 
					{
						if(board[row,col] == -1 && board[row,col + 1] == 0 && board[row,col + 2] == 0 && board[row,col + 3] == 0) 
							whoWon = whoWon - 100;//+
						else if(board[row,col] == 0 && board[row,col + 1] == -1 && board[row,col + 2] == 0 && board[row,col + 3] == 0) 
							whoWon = whoWon - 100;
						else if(board[row,col] == 0 && board[row,col + 1] == 0 && board[row,col + 2] == -1 && board[row,col + 3] == 0) 
							whoWon = whoWon - 100;
						else if(board[row,col] == 0 && board[row,col + 1] == 0 && board[row,col + 2] == 0 && board[row,col + 3] == -1) 
							whoWon = whoWon - 100;
						else if(board[row,col] == -1 && board[row,col + 1] == -1 && board[row,col + 2] == 0 && board[row,col + 3] == 0) 
							whoWon = whoWon - 1000;//+
						else if(board[row,col] == 0 && board[row,col + 1] == -1 && board[row,col + 2] == -1 && board[row,col + 3] == 0) 
							whoWon = whoWon - 1000;
						else if(board[row,col] == 0 && board[row,col + 1] == 0 && board[row,col + 2] == -1 && board[row,col + 3] == -1) 
							whoWon = whoWon - 1000;
						else if(board[row,col] == -1 && board[row,col + 1] == 0 && board[row,col + 2] == -1 && board[row,col + 3] == 0) 
							whoWon = whoWon - 1000;
						else if(board[row,col] == 0 && board[row,col + 1] == -1 && board[row,col + 2] == 0 && board[row,col + 3] == -1) 
							whoWon = whoWon - 1000;
						else if(board[row,col] == -1 && board[row,col + 1] == 0 && board[row,col + 2] == 0 && board[row,col + 3] == -1) 
							whoWon = whoWon - 1000;
						else if(board[row,col] == -1 && board[row,col + 1] == -1 && board[row,col + 2] == -1 && board[row,col + 3] == 0) 
							whoWon = whoWon - 10000;//+
						else if(board[row,col] == 0 && board[row,col + 1] == -1 && board[row,col + 2] == -1 && board[row,col + 3] == -1) 
							whoWon = whoWon - 10000;
						else if(board[row,col] == -1 && board[row,col + 1] == 0 && board[row,col + 2] == -1 && board[row,col + 3] == -1) 
							whoWon = whoWon - 10000;
						else if(board[row,col] == -1 && board[row,col + 1] == -1 && board[row,col + 2] == 0 && board[row,col + 3] == -1) 
							whoWon = whoWon - 10000;
						else if(board[row,col] == -1 && board[row,col + 1] == -1 && board[row,col + 2] == -1 && board[row,col + 3] == -1) 
							whoWon = whoWon - 500000;
					}
				}
				//Horizontal Player Two End



				//Vertical Player Two
				for(int col = 0; col < 7; col++)
				{
					for(int row = 5; row >= 3; row--)
					{
						if (board [row, col] == -1 && board [row - 1, col] == 0 && board [row - 2, col] == 0 && board [row - 3, col] == 0)
							whoWon = whoWon - 100;
						else if (board [row, col] == -1 && board [row - 1, col] == -1 && board [row - 2, col] == 0 && board [row - 3, col] == 0)
							whoWon = whoWon - 1000;
						else if (board [row, col] == -1 && board [row - 1, col] == -1 && board [row - 2, col] == -1 && board [row - 3, col] == 0)
							whoWon = whoWon - 10000;
						else if (board [row, col] == -1 && board [row - 1, col] == -1 && board [row - 2, col] == -1 && board [row - 3, col] == -1)
							whoWon = whoWon - 500000;
					}
				}
				//Vertical Player Two End



				//Diagonal Player Two
				for(int col = 0; col < 4; col++)
				{
					for(int row = 5; row >=3 ; row--)
					{
						if (board [row, col] == -1 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 0)
							whoWon = whoWon - 100;
						else if (board [row, col] == -1 && board [row - 1, col + 1] == -1 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 0)
							whoWon = whoWon - 1000;
						else if (board [row, col] == -1 && board [row - 1, col + 1] == -1 && board [row - 2, col + 2] == -1 && board [row - 3, col + 3] == 0)
							whoWon = whoWon - 10000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == -1 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == 0)
							whoWon = whoWon - 100;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == -1 && board [row - 2, col + 2] == -1 && board [row - 3, col + 3] == 0)
							whoWon = whoWon - 1000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == -1 && board [row - 2, col + 2] == -1 && board [row - 3, col + 3] == -1)
							whoWon = whoWon - 10000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == -1 && board [row - 3, col + 3] == 0)
							whoWon = whoWon - 100;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == -1 && board [row - 3, col + 3] == -1)
							whoWon = whoWon - 1000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == -1)
							whoWon = whoWon - 100;
						else if (board [row, col] == -1 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == -1 && board [row - 3, col + 3] == 0)
							whoWon = whoWon - 1000;
						else if (board [row, col] == -1 && board [row - 1, col + 1] == 0 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == -1)
							whoWon = whoWon - 1000;
						else if (board [row, col] == 0 && board [row - 1, col + 1] == -1 && board [row - 2, col + 2] == 0 && board [row - 3, col + 3] == -1)
							whoWon = whoWon - 1000;
						else if (board [row, col] == -1 && board [row - 1, col + 1] == -1 && board [row - 2, col + 2] == -1 && board [row - 3, col + 3] == -1)
							whoWon = whoWon - 500000;
					}
				}

				for(int col = 6; col >= 3; col--)
				{
					for(int row = 5; row >=3 ; row--)
					{
						if (board [row, col] == -1 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 0)
							whoWon = whoWon - 100;
						else if (board [row, col] == -1 && board [row - 1, col - 1] == -1 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 0)
							whoWon = whoWon - 1000;
						else if (board [row, col] == -1 && board [row - 1, col - 1] == -1 && board [row - 2, col - 2] == -1 && board [row - 3, col - 3] == 0)
							whoWon = whoWon - 10000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == -1 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == 0)
							whoWon = whoWon - 100;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == -1 && board [row - 2, col - 2] == -1 && board [row - 3, col - 3] == 0)
							whoWon = whoWon - 1000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == -1 && board [row - 2, col - 2] == -1 && board [row - 3, col - 3] == -1)
							whoWon = whoWon - 10000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == -1 && board [row - 3, col - 3] == 0)
							whoWon = whoWon - 100;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == -1 && board [row - 3, col - 3] == -1)
							whoWon = whoWon - 1000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == -1)
							whoWon = whoWon - 100;
						else if (board [row, col] == -1 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == -1 && board [row - 3, col - 3] == 0)
							whoWon = whoWon - 1000;
						else if (board [row, col] == -1 && board [row - 1, col - 1] == 0 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == -1)
							whoWon = whoWon - 1000;
						else if (board [row, col] == 0 && board [row - 1, col - 1] == -1 && board [row - 2, col - 2] == 0 && board [row - 3, col - 3] == -1)
							whoWon = whoWon - 1000;
						else if (board [row, col] == -1 && board [row - 1, col - 1] == -1 && board [row - 2, col - 2] == -1 && board [row - 3, col - 3] == -1)
							whoWon = whoWon - 500000;
					}
				}
				//Diagonal Player Two End



			return whoWon;
		}

		bool GameOver()
		{
			//Check for vertical win
			for(int c = 0;c < 7;c++) {
				for(int r = 5;r >= 3;r--) {
					if(board[r,c] == 1 && board[r - 1,c] == 1 && board[r - 2,c] == 1 && board[r - 3,c] == 1) {
						return true;
					} else if(board[r,c] == -1 && board[r - 1,c] == -1 && board[r - 2,c] == -1 && board[r - 3,c] == -1) {
						return true;
					}
				}
			}

			//check for horizontal win
			for(int r = 0;r < 6;r++) {
				for(int c = 0;c <= 3;c++) {
					if(board[r,c] == 1 && board[r,c + 1] == 1 && board[r,c + 2] == 1 && board[r,c + 3] == 1) {
						return true;
					} else if(board[r,c] == -1 && board[r,c + 1] == -1 && board[r,c + 2] == -1 && board[r,c + 3] == -1) {
						return true;
					}
				}
			}

			//check for diagonal win
			for(int r = 0;r <= 2;r++) {
				for(int c = 0;c < 4;c++) {
					if(board[r,c] == 1 && board[r + 1,c + 1] == 1 && board[r + 2,c + 2] == 1 && board[r + 3,c + 3] == 1) {
						return true;
					} else if(board[r,c] == -1 && board[r + 1,c + 1] == -1 && board[r + 2,c + 2] == -1 && board[r + 3,c + 3] == -1) {
						return true;
					}
				}
			}

			for(int r = 0;r <= 2;r++) {
				for(int c = 6;c >= 3;c--) {
					if(board[r,c] == 1 && board[r + 1,c - 1] == 1 && board[r + 2,c - 2] == 1 && board[r + 3,c - 3] == 1) {
						return true;
					} else if(board[r,c] == -1 && board[r + 1,c - 1] == -1 && board[r + 2,c - 2] == -1 && board[r + 3,c - 3] == -1) {
						return true;
					}
				}
			}

			return false;
		}

		public int[,] findAllLegalMoves() 
		{
			int[,] legalMove = new int[7,2]{
				{-1,-1},
				{-1,-1},
				{-1,-1},
				{-1,-1},
				{-1,-1},
				{-1,-1},
				{-1,-1}
			};

			for(int c = 0;c < 7;c++) 
			{
				for(int r = 5;r >= 0;r--) 
				{
					if(board[r,c] == 0) 
					{
						legalMove[c,0] = r;
						legalMove[c,1] = c;
						break;
					}
				}
			}
			return legalMove;
		}

		int NegaMax(int[,] board, int depth, int color, int alpha, int beta) 
		{
			/*
			if (GameOver() || depth>maxDepth)
				return color*analysis();
			*/

			if(GameOver() || (DateTime.Now - currentTime).TotalSeconds > 8 || depth > maxDepth)
				return color*analysis();
			
			int max = -1000000;

			int[,] legalMove = findAllLegalMoves ();

			for(int move = 0; move < 7; move++)
			{
				if((DateTime.Now - currentTime).TotalSeconds > 8 && depth == 0 && depthShown == false)
				{
					//Debug.Log ("Move: " + move);
					depthShown = true;
				}
				if(legalMove[move,0] == -1 || legalMove[move,1] == -1) 
					continue;
				else
				{
					board[legalMove[move,0],legalMove[move,1]] = color;
					int temp = -NegaMax(board, depth + 1, color*(-1), -beta, -alpha);
					//Debug.Log (temp + " " + depth);
					board[legalMove[move,0],legalMove[move,1]] = 0;

					if(temp > max) 
					{
						max = temp;
						if(depth == 0) {
							bestMove[0,0] = legalMove[move,0];
							bestMove[0,1] = legalMove[move,1];
						}
					}
					if(temp > alpha) {
						alpha = temp; 
					}
					if(alpha >= beta) {
						return alpha;
					}
				}

			}

			/*
			for each legal move m in board b 
			{
				copy b to c
				make move m in board c
				int x= - NegaMax(c, depth+1, -color, -beta, -alpha)
					if (x>max) max = x
					if (x>alpha) alpha = x
					if (alpha>=beta) return alpha
			}*/
			return max;
		}
	}
}
