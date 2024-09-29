import React from 'react';
import { View, Button, StyleSheet, Alert } from 'react-native';
import RNHTMLtoPDF from 'react-native-html-to-pdf';

interface PdfGeneratorProps {
  fields: { title: string; value: string }[];
}

const PdfGenerator: React.FC<PdfGeneratorProps> = ({ fields }) => {
    console.log('fields', fields)
  const createPDF = async () => {
    const html = `
      <h1>Added Fields</h1>
      <ul>
        ${fields.map(field => `<li>${field.title}: ${field.value}</li>`).join('')}
      </ul>
    `;

    const options = {
      html,
      fileName: 'AddedFields',
      directory: 'Documents',
    };
    console.log('options', options)

    try {
      const file = await RNHTMLtoPDF.convert(options);
      console.log(file.filePath); // Path to the generated PDF
      Alert.alert('PDF generated', 'PDF generated: ' + file.filePath);
    } catch (error) {
      console.error(error);
      Alert.alert('Error', 'Failed to generate PDF');
    }
  };

  return (
    <View style={styles.container}>
      <Button title="Download PDF" onPress={createPDF} />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    marginTop: 20,
  },
});

export default PdfGenerator;
