#include <windows.h>
#include <string>
#include "gtest/gtest.h"
#include "../LibProject/Lib.h"
#include "../../GoogleTestAdapter/Core/Resources/GTA_Traits.h"

extern std::string TEST_DIRECTORY;

// http://stackoverflow.com/questions/8233842/how-to-check-if-directory-exist-using-c-and-winapi
bool DirExists(const std::string& dirName_in)
{
	DWORD ftyp = GetFileAttributesA(dirName_in.c_str());
	if (ftyp == INVALID_FILE_ATTRIBUTES)
		return false;  //something is wrong with your path!

	if (ftyp & FILE_ATTRIBUTE_DIRECTORY)
		return true;   // this is a directory!

	return false;    // this is not a directory!
}

TEST(CommandArgs, TestDirectoryIsSet)
{
	ASSERT_STRNE("", TEST_DIRECTORY.c_str());
	ASSERT_TRUE(DirExists(TEST_DIRECTORY));
}

TEST(TestMath, AddFails)
{
	EXPECT_EQ(1000, Add(10, 10));
}

TEST(TestMath, AddPasses)
{
	EXPECT_EQ(20, Add(10, 10));
}

TEST(TestMath, Crash)
{
	int* pInt = NULL;
	EXPECT_EQ(20, Add(*pInt, 10));
}

TEST_TRAITS(TestMath, AddPassesWithTraits, Type, Medium)
{
	EXPECT_EQ(20, Add(10, 10));
}

TEST_TRAITS(Traits, With8Traits, Trait1, Equals1, Trait2, Equals2, Trait3, Equals3, Trait4, Equals4, Trait5, Equals5, Trait6, Equals6, Trait7, Equals7, Trait8, Equals8)
{
	EXPECT_EQ(1, 1);
}

TEST_TRAITS(Traits, With7Traits, Trait1, Equals1, Trait2, Equals2, Trait3, Equals3, Trait4, Equals4, Trait5, Equals5, Trait6, Equals6, Trait7, Equals7)
{
	EXPECT_EQ(1, 1);
}

TEST_TRAITS(Traits, With6Traits, Trait1, Equals1, Trait2, Equals2, Trait3, Equals3, Trait4, Equals4, Trait5, Equals5, Trait6, Equals6)
{
	EXPECT_EQ(1, 1);
}

TEST_TRAITS(Traits, With5Traits, Trait1, Equals1, Trait2, Equals2, Trait3, Equals3, Trait4, Equals4, Trait5, Equals5)
{
	EXPECT_EQ(1, 1);
}

TEST_TRAITS(Traits, With4Traits, Trait1, Equals1, Trait2, Equals2, Trait3, Equals3, Trait4, Equals4)
{
	EXPECT_EQ(1, 1);
}

TEST_TRAITS(Traits, With3Traits, Trait1, Equals1, Trait2, Equals2, Trait3, Equals3)
{
	EXPECT_EQ(1, 1);
}

TEST_TRAITS(Traits, With2Traits, Trait1, Equals1, Trait2, Equals2)
{
	EXPECT_EQ(1, 1);
}

TEST_TRAITS(Traits, With1Traits, Trait1, Equals1)
{
	EXPECT_EQ(1, 1);
}

TEST_TRAITS(Fixt�re, T�stWithUml�uten, Tr�it, Val�e)
{
	EXPECT_EQ(1, 0) << "Te�t failed";
}
