#include "glSetup.h"

#include <vector>
#include <math.h>
#include <iostream>
using namespace std;

#include <Eigen/Dense>
using namespace Eigen;

void render(GLFWwindow* window);
void keyboard(GLFWwindow* window, int key, int code, int action, int mods);
void mouse(GLFWwindow* window, int button, int action, int mods);
void drag(GLFWwindow* window, double x, double y);

GLfloat bgColor[4] = { 1, 1, 1, 1 };

Vector3f ee_goal;

int condition = 0;
int selectedPoint;
bool isDragged = false;

int main(int argc, char* argv[])
{
	perspectiveView = false;

	GLFWwindow* window = initializeOpenGL(argc, argv, bgColor);
	if (window == NULL) return -1;

	glfwSetKeyCallback(window, keyboard);
	glfwSetCursorPosCallback(window, drag);
	glfwSetMouseButtonCallback(window, mouse);

	glDisable(GL_DEPTH_TEST);

	reshape(window, windowW, windowH);

	while (!glfwWindowShouldClose(window))
	{
		render(window);
		glfwSwapBuffers(window);
		glfwPollEvents();
	}
	glfwDestroyWindow(window);
	glfwTerminate();
}

vector<Vector3f> p;

//Ac = b
MatrixXf A; //4n x 4n
MatrixXf b; //4n x 3
MatrixXf c; //4n x 3

void buildLinearSystem()
{
	A.resize(4 * p.size(), 4 * p.size());
	A.setZero();

	b.resize(4 * p.size(), 3);

	int row = 0;

	// 2n 
	for (int i = 0; i < p.size() - 1; i++, row += 2)
	{
		A(row, 4 * i + 0) = 1;
		
		b(row, 0) = p[i][0]; //x
		b(row, 1) = p[i][1]; //y
		b(row, 2) = p[i][2]; //z

		A(row + 1, 4 * i + 0) = 1;
		A(row + 1, 4 * i + 1) = 1;
		A(row + 1, 4 * i + 2) = 1;
		A(row + 1, 4 * i + 3) = 1;

		b(row + 1, 0) = p[i + 1][0];
		b(row + 1, 1) = p[i + 1][1];
		b(row + 1, 2) = p[i + 1][2];
	}

	// n-1 tangential
	for (int i = 0; i < p.size() - 2; i++, row++)
	{
		A(row, 4 * i + 1) = 1;
		A(row, 4 * i + 2) = 2;
		A(row, 4 * i + 3) = 3;
		A(row, 4 * i + 5) = -1;
		
		b(row, 0) = 0;
		b(row, 1) = 0;
		b(row, 2) = 0;
	}

	// n-1 second-derivative
	for (int i = 0; i < p.size() - 2; i++, row++)
	{
		A(row, 4 * i + 2) = 2;
		A(row, 4 * i + 3) = 6;
		A(row, 4 * i + 6) = -2;

		b(row, 0) = 0;
		b(row, 1) = 0;
		b(row, 2) = 0;
	}

	// 2 for natural boundary condition
	{
		A(row, 2) = 2;

		b(row, 0) = 0;
		b(row, 1) = 0;
		b(row, 2) = 0;

		row++;

		A(row, 4 * (p.size() - 2) + 2) = 2;
		A(row, 4 * (p.size() - 2) + 3) = 6;

		b(row, 0) = 0;
		b(row, 1) = 0;
		b(row, 2) = 0;

		row++;
	}
}

void solveLinearSystem()
{
	c = A.colPivHouseholderQr().solve(b);
}

void drawNaturalCubicSpline()
{
	int N_SUB_SEGMENTS = 40;

	//segment curve
	glLineWidth(1.5*dpiScaling);
	glColor3f(0, 0, 0);
	for (int i = 0; i < p.size(); i++)
	{
		glBegin(GL_LINE_STRIP);
		for (int j = 0; j < N_SUB_SEGMENTS; j++)
		{
			float t = (float)j / (N_SUB_SEGMENTS - 1);

			float x = c(4 * i, 0) + (c(4 * i + 1, 0) + (c(4 * i + 2, 0) + c(4 * i + 3, 0)*t)*t)*t;
			float y = c(4 * i, 1) + (c(4 * i + 1, 1) + (c(4 * i + 2, 1) + c(4 * i + 3, 1)*t)*t)*t;
			float z = c(4 * i, 2) + (c(4 * i + 1, 2) + (c(4 * i + 2, 2) + c(4 * i + 3, 2)*t)*t)*t;

			glVertex3f(x, y, z);
		}
		glEnd();
	}

	//point
	glPointSize(10);
	glColor3f(1, 0, 0);
	glBegin(GL_POINTS);
	for (int i = 0; i < p.size(); i++)
		glVertex3f(p[i][0], p[i][1], 0);
	glEnd();
}

void render(GLFWwindow* window)
{
	glClearColor(bgColor[0], bgColor[1], bgColor[2], bgColor[3]);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();

	if (p.size() > 1)
		drawNaturalCubicSpline();
}

void keyboard(GLFWwindow* window, int key, int scancode, int action, int mods)
{
	if (action == GLFW_PRESS || action == GLFW_REPEAT)
	{
		switch (key)
		{
			//Quit
		case GLFW_KEY_Q:
		case GLFW_KEY_ESCAPE: glfwSetWindowShouldClose(window, GL_TRUE); break;
		case GLFW_KEY_A: condition = 1; cout << "Add Point" << endl; break;
		case GLFW_KEY_R: condition = 2; cout << "select/remove" << endl; break;
		case GLFW_KEY_D: condition = 3; cout << "select/drag" << endl; break;
		case GLFW_KEY_I: condition = 4; cout << "select/insert" << endl; break;
		}
	}
}

void setPoint()
{
	if (p.size() > 1)
	{
		buildLinearSystem();
		solveLinearSystem();
	}
}

int selectPoint(Vector3f vec)
{
	int index;
	float min = 100;

	//find nearest point
	for (int i = 0; i < p.size(); i++)
	{
		float x, y;
		x = (p[i][0] - vec[0]) * (p[i][0] - vec[0]);
		y = (p[i][1] - vec[1]) * (p[i][1] - vec[1]);
		if (min > x + y)
		{
			min = x + y;
			index = i;
		}
	}
	return index;
}

void drag(GLFWwindow* window, double x, double y)
{
	if (isDragged)
	{
		float aspect = (float)screenW / screenH;
		x = 2.0 * (x / screenW - 0.5) * aspect;
		y = -2.0 * (y / screenH - 0.5);
		ee_goal[0] += x - ee_goal[0];
		ee_goal[1] += y - ee_goal[1];
		Vector3f newPosition = Vector3f(ee_goal[0], ee_goal[1], .0);
		p[selectedPoint] = newPosition;
		setPoint();
	}
}

void selectCurve()
{
	Vector3f index;
	int idxPoint;
	float min = 100;
	float length;

	//find Point
	for (int i = 0; i < p.size() - 1; i++)
	{
		Vector3f beforePoint;
		
		for (int j = 0; j < 40; j++)
		{
			float t = (float)j / 39;

			float x = c(4 * i, 0) + (c(4 * i + 1, 0) + (c(4 * i + 2, 0) + c(4 * i + 3, 0)*t)*t)*t;
			float y = c(4 * i, 1) + (c(4 * i + 1, 1) + (c(4 * i + 2, 1) + c(4 * i + 3, 1)*t)*t)*t;
			
			if (j != 0)
			{
				// vector : before Point to next Point
				float vectorX = x - beforePoint[0]; 
				float vectorY = y - beforePoint[1];
				
				float line = sqrt(vectorX * vectorX + vectorY * vectorY);

				// vector : before Point to cursor Point
				float cursorX = ee_goal[0] - beforePoint[0];
				float cursorY = ee_goal[1] - beforePoint[1];

				// vector : before Point to cursor Point
				float nowX = ee_goal[0] - x;
				float nowY = ee_goal[1] - y;

				float dot = vectorX * cursorX + vectorY * cursorY;
				
				//find dot Proj(a,b) = dot1/(line*line)*vector
				float pointX, pointY;
				//beforePoint = nowPoint, line = 0
				if (line == 0)
				{
					pointX = beforePoint[0];
					pointY = beforePoint[1];
				}
				else
				{
					pointX = beforePoint[0] + dot / (line * line) * vectorX;
					pointY = beforePoint[1] + dot / (line * line) * vectorY;
				}

				//range check
				float cos=0;
				if (beforePoint[0] < pointX && pointX < x)
				{
					if (beforePoint[1] < pointY && pointY < y)
						cos = 1;
				}
				else if (x < pointX && pointX < beforePoint[0])
				{
					if (y < pointY && pointY < beforePoint[1])
						cos = 1;
				}

				//point is inline
				if (cos == 1)
				{
					// proj : dot(vector,cursor) / length(vector)
					float proj = fabs(dot / line);

					if (isnan(proj) != 0) //vector Point == proj Point H, When line = 0
						proj = 0;

					// length : cursor^2 - proj^2
					length = sqrt(cursorX * cursorX + cursorY * cursorY - proj * proj);
				}
				else
				{
					beforePoint = Vector3f(x, y, 0);

					float beforeLength = sqrt(cursorX * cursorX + cursorY * cursorY);
					float nowLength = sqrt(nowX * nowX + nowY * nowY);
					//select close Point
					if (beforeLength <= nowLength)
					{
						length = nowLength;
						pointX = x;
						pointY = y;
					}
					else
					{
						length = beforeLength;
						pointX = beforePoint[0];
						pointY = beforePoint[1];
					}

				}

				if (min > length)
				{
					min = length;

					index = Vector3f(pointX, pointY, 0);
					idxPoint = i;
				}
			}
			beforePoint = Vector3f(x, y, 0);
		}
	}

	//insert Point
	p.insert(p.begin() + idxPoint + 1, index);
}

void mouse(GLFWwindow* window, int button, int action, int mods)
{
	if (button == GLFW_MOUSE_BUTTON_LEFT)
	{
		if (action == GLFW_PRESS)
		{
			//find cursor
			double xpos, ypos;
			glfwGetCursorPos(window, &xpos, &ypos);
			ee_goal = Vector3f(xpos, ypos, 0);

			float aspect = (float)screenW / screenH;
			ee_goal[0] = 2.0 * (ee_goal[0] / screenW - 0.5) * aspect;
			ee_goal[1] = -2.0 * (ee_goal[1] / screenH - 0.5);

			switch (condition)
			{
			//a key
			case 1:
			{
				p.push_back(ee_goal);
				break;
			}
			//r key
			case 2:
			{
				int idx = selectPoint(ee_goal);
				p.erase(p.begin() + idx);
				break;
			}
			//d key
			case 3:
			{
				isDragged = true;
				selectedPoint = selectPoint(ee_goal);
				break;
			}
			//i key
			case 4:
			{
				selectCurve();
				break;
			}

			}
		}
		if (action == GLFW_RELEASE)
			isDragged = false;
	}

	setPoint();
}
