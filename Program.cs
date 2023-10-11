using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HW_30._09._23
{
    internal class Program
    {

        static void Main(string[] args)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LibraryConn"].ConnectionString))
            {
                conn.Open();
                //1. Выведите список должников
                SqlCommand debtors = new SqlCommand("SELECT first_name, last_name, 'Student' as person_type FROM Student " +
                "JOIN S_Cards ON S_Cards.id_student = Student.id WHERE date_in IS NULL " +
                "UNION ALL " +
                "SELECT first_name, last_name, 'Teacher' as person_type FROM Teacher " +
                "JOIN T_Cards ON T_Cards.id_teacher = Teacher.id WHERE date_in IS NULL", conn);
                SqlDataReader reader = debtors.ExecuteReader();
                Console.WriteLine("\tСписок должников:");
                while (reader.Read())
                {
                    string personType = reader["person_type"].ToString();
                    string firstName = reader["first_name"].ToString();
                    string lastName = reader["last_name"].ToString();

                    Console.WriteLine($"{personType}: {firstName} {lastName}");
                }
                reader.Close();

                //2. Выведите список авторов книги №3 (по порядку из таблицы ‘Book’). 
                Console.Write("\n\n\tАвтор книги под номером 3:");
                SqlCommand autorsBookNumThree = new SqlCommand("SELECT first_name, last_name "+
                "FROM Book " +
                "JOIN Author ON Author.id = Book.id_author " +
                "WHERE Book.id = 3", conn);
                reader=autorsBookNumThree.ExecuteReader();
                while (reader.Read()) 
                { 
                    //string author_name = reader["author_name"].ToString();
                    string firstName = reader["first_name"].ToString();
                    string lastName = reader["last_name"].ToString();

                    Console.Write($" {firstName} {lastName}");
                }
                reader.Close();

                //3. Выведите список книг, которые доступны в данный момент. 
                Console.WriteLine("\n\n\tСписок книг, которые доступны в данный момент:");
                SqlCommand listAvailableBooks = new SqlCommand("SELECT id, name FROM Book " +
                    "WHERE quantity>0", conn);
                reader=listAvailableBooks.ExecuteReader();
                
                while (reader.Read()) 
                {
                    //Console.WriteLine($"\t{reader["id"]}. {reader["name"]}");
                    Console.WriteLine($"\t{reader[0].ToString()}. {reader[1].ToString()}");
                }
                reader.Close();

                //4. Вывести список книг, которые на руках у пользователя №2. 
                Console.WriteLine("\n\n\tСписок книг, которые на руках у пользователя 2:");
                SqlCommand booksUser2 = new SqlCommand("SELECT Book.name " +
                    "FROM Book " +
                    "JOIN S_Cards ON Book.id = S_Cards.id_book " +
                    "JOIN Student ON S_Cards.id_student = Student.id " +
                    "JOIN T_Cards ON Book.id = T_Cards.id_book " +
                    "JOIN Teacher ON T_Cards.id_teacher = Teacher.id " +
                    "WHERE (Student.id = 2 OR Teacher.id = 2)", conn);

                reader = booksUser2.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"\t{reader[0].ToString()}");
                }
                reader.Close();

                //5. Вывести список книг, которые были взяты за последние 2 недели.
                Console.WriteLine("\n\n\tСписок книг, взятых за последние две недели:");
                // Вычисление даты, предшествующей текущей дате на 2 недели
                DateTime twoWeeksAgo = DateTime.Now.AddDays(-14);

                SqlCommand booksTakenLastTwoWeeks = new SqlCommand("SELECT Book.name " +
                    "FROM Book " +
                    "JOIN S_Cards ON Book.id = S_Cards.id_book " +
                    "JOIN Student ON S_Cards.id_student = Student.id " +
                    "WHERE S_Cards.date_out >= @TwoWeeksAgo", conn);

                // Добавление параметра @TwoWeeksAgo с вычисленной датой
                booksTakenLastTwoWeeks.Parameters.Add(new SqlParameter("@TwoWeeksAgo", twoWeeksAgo));

                reader = booksTakenLastTwoWeeks.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"\t{reader[0].ToString()}");
                }
                reader.Close();

                //6. Обнулите задолженности всех должников.
                SqlCommand resetDebts = new SqlCommand("UPDATE S_Cards " +
                "SET date_in = GETDATE() " +
                "WHERE date_in IS NULL", conn);

                int rowsAffected = resetDebts.ExecuteNonQuery();

                Console.WriteLine($"\tОбнулено {rowsAffected} задолженностей.");

                //7. Вывести количество книг, взятых определённым посетителем за последний год.
                Console.WriteLine("\n\nКоличество книг, взятых определенным посетителем за последний год:");
               
                Console.Write("\n\tВведите id студента: ");
                int visitorID = int.Parse(Console.ReadLine());
               
                // Получение текущей даты
                DateTime currentDate = DateTime.Now;

                // Вычисление даты, предшествующей текущей дате на 1 год
                DateTime oneYearAgo = currentDate.AddYears(-1);

                SqlCommand booksTakenLastYear = new SqlCommand("SELECT COUNT(*) " +
                    "FROM S_Cards " +
                    "WHERE id_student = @VisitorID AND date_out >= @OneYearAgo", conn);

                booksTakenLastYear.Parameters.Add(new SqlParameter("@VisitorID", visitorID));
                booksTakenLastYear.Parameters.Add(new SqlParameter("@OneYearAgo", oneYearAgo));

                int booksCount = (int)booksTakenLastYear.ExecuteScalar();

                Console.WriteLine($"Посетитель {visitorID} взял {booksCount} книг за последний год.");

                Console.WriteLine("\n\n\tАвтор книги под номером 3 + Список книг, которые доступны в данный момент:");
                SqlCommand autorsBookNumThreeAndArrowBook = new SqlCommand(
                    "SELECT first_name, last_name FROM Book " +
                    "JOIN Author ON Author.id = Book.id_author " +
                    "WHERE Book.id = 3; " +
                    "SELECT id, name FROM Book WHERE quantity > 0", conn);

                reader = autorsBookNumThreeAndArrowBook.ExecuteReader();

                do
                {
                    int line = 0;
                    while (reader.Read())
                    {
                        if (line == 0)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader.GetName(i)} ");
                            }
                            Console.WriteLine();
                        }
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader[i]} ");
                        }
                        Console.WriteLine();
                        line++;
                    }
                    
                } while (reader.NextResult());

            }


        }
        }
    }

